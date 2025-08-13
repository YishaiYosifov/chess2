using Chess2.Api.GameSnapshot.Models;
using Chess2.Api.Infrastructure.SignalR;
using Chess2.Api.TestInfrastructure;
using Chess2.Api.TestInfrastructure.Fakes;
using Chess2.Api.TestInfrastructure.Utils;
using FluentAssertions;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;

namespace Chess2.Api.Functional.Tests.LobbyTests;

public class LobbyHubTests(Chess2WebApplicationFactory factory) : BaseFunctionalTest(factory)
{
    private const string HubPath = "/api/hub/lobby";
    private const string SeekCasualMethod = "SeekCasualAsync";
    private const string SeekRatedMethod = "SeekRatedAsync";

    [Fact]
    public async Task Connecting_without_access_token_throws_error()
    {
        var act = async () => await ConnectSignalRAsync(HubPath);
        await act.Should().ThrowAsync<HttpRequestException>().WithMessage("*Unauthorized*");
    }

    [Fact]
    public async Task SeekCasualAsync_guest_vs_guest_matches()
    {
        await using var conn1 = await ConnectSignalRGuestAsync(HubPath, "guest1");
        await using var conn2 = await ConnectSignalRGuestAsync(HubPath, "guest2");

        await AssertPlayersMatchAsync(
            conn1,
            conn2,
            new TimeControlSettings(600, 0),
            SeekCasualMethod
        );
    }

    [Fact]
    public async Task SeekRatedAsync_match_with_two_authed_users()
    {
        var user1 = await FakerUtils.StoreFakerAsync(DbContext, new AuthedUserFaker());
        var user2 = await FakerUtils.StoreFakerAsync(DbContext, new AuthedUserFaker());

        await using var conn1 = await ConnectSignalRAuthedAsync(HubPath, user1);
        await using var conn2 = await ConnectSignalRAuthedAsync(HubPath, user2);

        await AssertPlayersMatchAsync(
            conn1,
            conn2,
            new TimeControlSettings(300, 10),
            SeekRatedMethod
        );
    }

    [Fact]
    public async Task SeekCasualAsync_match_with_authed_and_guest()
    {
        var authedUser = await FakerUtils.StoreFakerAsync(DbContext, new AuthedUserFaker());

        await using var conn1 = await ConnectSignalRAuthedAsync(HubPath, authedUser);
        await using var conn2 = await ConnectSignalRGuestAsync(HubPath, "guest1");

        await AssertPlayersMatchAsync(
            conn1,
            conn2,
            new TimeControlSettings(900, 3),
            SeekCasualMethod
        );
    }

    [Fact]
    public async Task SeekRatedAsync_and_SeekCasualAsync_with_multiple_concurrent_user_pairs()
    {
        var timeControl = new TimeControlSettings(600, 5);

        var authed1 = await FakerUtils.StoreFakerAsync(DbContext, new AuthedUserFaker());
        var authed2 = await FakerUtils.StoreFakerAsync(DbContext, new AuthedUserFaker());
        var authed3 = await FakerUtils.StoreFakerAsync(DbContext, new AuthedUserFaker());
        var authed4 = await FakerUtils.StoreFakerAsync(DbContext, new AuthedUserFaker());

        await using var authedConn1 = await ConnectSignalRAuthedAsync(HubPath, authed1);
        await using var authedConn2 = await ConnectSignalRAuthedAsync(HubPath, authed2);
        await using var authedConn3 = await ConnectSignalRAuthedAsync(HubPath, authed3);
        await using var authedConn4 = await ConnectSignalRAuthedAsync(HubPath, authed4);

        await using var guestConn1 = await ConnectSignalRGuestAsync(HubPath, "guest1");
        await using var guestConn2 = await ConnectSignalRGuestAsync(HubPath, "guest2");

        var concurrentRatedMatchTask = AssertConcurrentMatchesAsync(
            timeControl,
            SeekRatedMethod,
            authedConn1,
            authedConn2,
            authedConn3,
            authedConn4
        );
        var guestMatchTask = AssertPlayersMatchAsync(
            guestConn1,
            guestConn2,
            timeControl,
            SeekCasualMethod
        );

        await Task.WhenAll(concurrentRatedMatchTask, guestMatchTask);
    }

    [Fact]
    public async Task SeekRated_with_a_guest_should_return_an_error()
    {
        var conn = await ConnectSignalRGuestAsync(HubPath, "guest1");

        var tsc = new TaskCompletionSource<IEnumerable<SignalRError>>();
        conn.On<IEnumerable<SignalRError>>("ReceiveErrorAsync", errors => tsc.TrySetResult(errors));

        await conn.InvokeAsync(SeekRatedMethod, new TimeControlSettings(300, 10), CT);

        var result = await tsc.Task.WaitAsync(TimeSpan.FromSeconds(10), CT);
        result.Should().ContainSingle().Which.Code.Should().Be("General.Unauthorized");
    }

    [Fact]
    public async Task Seek_and_disconnect_cancels_the_seek()
    {
        var timeControl = new TimeControlSettings(300, 10);

        await using var conn1 = await ConnectSignalRGuestAsync(HubPath, "guest1");
        await conn1.InvokeAsync(SeekCasualMethod, timeControl, CT);
        await conn1.StopAsync(CT);

        await using var conn2 = await ConnectSignalRGuestAsync(HubPath, "guest2");
        await using var conn3 = await ConnectSignalRGuestAsync(HubPath, "guest3");

        // users are matched in the order they connected, so if conn1 disconnects, conn2 and conn3 should match
        await AssertPlayersMatchAsync(conn2, conn3, timeControl, SeekCasualMethod);
    }

    [Fact]
    public async Task Seek_and_disconnect_on_another_connection_doesnt_cancel_the_seek()
    {
        var timeControl = new TimeControlSettings(300, 10);

        await using var guest1ActiveConn = await ConnectSignalRGuestAsync(HubPath, "guest1");
        await using var guest1DisconnectedConn = await ConnectSignalRGuestAsync(HubPath, "guest1");
        await guest1ActiveConn.InvokeAsync(SeekCasualMethod, timeControl, CT);
        await guest1DisconnectedConn.StopAsync(CT);

        await using var guest2Conn = await ConnectSignalRGuestAsync(HubPath, "guest2");
        await guest2Conn.InvokeAsync(SeekCasualMethod, timeControl, CT);

        await AssertMatchEstablishedAsync(guest1ActiveConn, guest2Conn);
    }

    private async Task AssertConcurrentMatchesAsync(
        TimeControlSettings timeControl,
        string methodName,
        params List<HubConnection> conns
    )
    {
        List<TaskCompletionSource<string>> tcsList = [];
        foreach (var conn in conns)
        {
            tcsList.Add(ListenForMatch(conn));
            await conn.InvokeAsync(methodName, timeControl);
        }

        var tokens = await Task.WhenAll(
            tcsList.Select(tcs => tcs.Task.WaitAsync(TimeSpan.FromSeconds(10), CT))
        );
        var tokenCounts = tokens.GroupBy(token => token).ToDictionary(g => g.Key, g => g.Count());

        tokenCounts.Should().HaveCount(conns.Count / 2);
        foreach (var kvp in tokenCounts)
        {
            kvp.Value.Should().Be(2);
            kvp.Key.Should().NotBeNullOrEmpty().And.HaveLength(16);
        }
    }

    private async Task<string> AssertPlayersMatchAsync(
        HubConnection conn1,
        HubConnection conn2,
        TimeControlSettings timeControl,
        string methodName
    )
    {
        await conn1.InvokeAsync(methodName, timeControl);
        await conn2.InvokeAsync(methodName, timeControl);

        var gameToken = await AssertMatchEstablishedAsync(conn1, conn2);
        return gameToken;
    }

    private async Task<string> AssertMatchEstablishedAsync(HubConnection conn1, HubConnection conn2)
    {
        var tcs1 = ListenForMatch(conn1);
        var tcs2 = ListenForMatch(conn2);

        var timeout = TimeSpan.FromSeconds(10);
        var gameToken1 = await tcs1.Task.WaitAsync(timeout, CT);
        var gameToken2 = await tcs2.Task.WaitAsync(timeout, CT);

        gameToken1.Should().NotBeNullOrEmpty().And.HaveLength(16).And.Be(gameToken2);

        return gameToken1;
    }

    private static TaskCompletionSource<string> ListenForMatch(HubConnection conn)
    {
        var tcs = new TaskCompletionSource<string>();
        conn.On<string>("MatchFoundAsync", gameToken => tcs.TrySetResult(gameToken));
        return tcs;
    }
}
