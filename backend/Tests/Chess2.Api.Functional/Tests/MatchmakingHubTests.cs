using Chess2.Api.Game.Models;
using Chess2.Api.Infrastructure.SignalR;
using Chess2.Api.TestInfrastructure;
using Chess2.Api.TestInfrastructure.Fakes;
using Chess2.Api.TestInfrastructure.Utils;
using FluentAssertions;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;

namespace Chess2.Api.Functional.Tests;

public class MatchmakingHubTests(Chess2WebApplicationFactory factory) : BaseFunctionalTest(factory)
{
    private const string MatchmakingHubPath = "/api/hub/matchmaking";
    private const string SeekCasualMethod = "SeekCasualAsync";
    private const string SeekRatedMethod = "SeekRatedAsync";

    [Fact]
    public async Task Connecting_without_access_token_throws_error()
    {
        var act = async () => await ConnectSignalRAsync(MatchmakingHubPath);
        await act.Should().ThrowAsync<HttpRequestException>().WithMessage("*Unauthorized*");
    }

    [Fact]
    public async Task SeekCasualAsync_guest_vs_guest_matches()
    {
        await using var conn1 = await ConnectSignalRGuestAsync(MatchmakingHubPath, "guest1");
        await using var conn2 = await ConnectSignalRGuestAsync(MatchmakingHubPath, "guest2");

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

        await using var conn1 = await ConnectSignalRAuthedAsync(MatchmakingHubPath, user1);
        await using var conn2 = await ConnectSignalRAuthedAsync(MatchmakingHubPath, user2);

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

        await using var conn1 = await ConnectSignalRAuthedAsync(MatchmakingHubPath, authedUser);
        await using var conn2 = await ConnectSignalRGuestAsync(MatchmakingHubPath, "guest1");

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

        var user1Match1 = await FakerUtils.StoreFakerAsync(DbContext, new AuthedUserFaker());
        var user2Match1 = await FakerUtils.StoreFakerAsync(DbContext, new AuthedUserFaker());

        var user1Match2 = await FakerUtils.StoreFakerAsync(DbContext, new AuthedUserFaker());
        var user2Match2 = await FakerUtils.StoreFakerAsync(DbContext, new AuthedUserFaker());

        await using var conn1Match1 = await ConnectSignalRAuthedAsync(
            MatchmakingHubPath,
            user1Match1
        );
        await using var conn2Match1 = await ConnectSignalRAuthedAsync(
            MatchmakingHubPath,
            user2Match1
        );

        await using var conn1Match2 = await ConnectSignalRAuthedAsync(
            MatchmakingHubPath,
            user1Match2
        );
        await using var conn2Match2 = await ConnectSignalRAuthedAsync(
            MatchmakingHubPath,
            user2Match2
        );

        await using var conn1Match3 = await ConnectSignalRGuestAsync(MatchmakingHubPath, "guest1");
        await using var conn2Match3 = await ConnectSignalRGuestAsync(MatchmakingHubPath, "guest2");

        var match1AssertTask = AssertPlayersMatchAsync(
            conn1Match1,
            conn2Match1,
            timeControl,
            SeekRatedMethod
        );
        var match2AssertTask = AssertPlayersMatchAsync(
            conn1Match2,
            conn2Match2,
            timeControl,
            SeekRatedMethod
        );
        var match3AssertTask = AssertPlayersMatchAsync(
            conn1Match3,
            conn2Match3,
            timeControl,
            SeekCasualMethod
        );

        await Task.WhenAll(match1AssertTask, match2AssertTask, match3AssertTask);
    }

    [Fact]
    public async Task SeekRated_with_a_guest_should_return_an_error()
    {
        var conn = await ConnectSignalRGuestAsync(MatchmakingHubPath, "guest1");

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

        await using var conn1 = await ConnectSignalRGuestAsync(MatchmakingHubPath, "guest1");
        await conn1.InvokeAsync(SeekCasualMethod, timeControl, CT);
        await conn1.StopAsync(CT);

        await using var conn2 = await ConnectSignalRGuestAsync(MatchmakingHubPath, "guest2");
        await using var conn3 = await ConnectSignalRGuestAsync(MatchmakingHubPath, "guest3");

        // users are matched in the order they connected, so if conn1 disconnects, conn2 and conn3 should match
        await AssertPlayersMatchAsync(conn2, conn3, timeControl, SeekCasualMethod);
    }

    [Fact]
    public async Task Seek_and_disconnect_on_another_connection_doesnt_cancel_the_seek()
    {
        var timeControl = new TimeControlSettings(300, 10);

        await using var guest1ActiveConn = await ConnectSignalRGuestAsync(
            MatchmakingHubPath,
            "guest1"
        );
        await using var guest1DisconnectedConn = await ConnectSignalRGuestAsync(
            MatchmakingHubPath,
            "guest1"
        );
        await guest1ActiveConn.InvokeAsync(SeekCasualMethod, timeControl, CT);
        await guest1DisconnectedConn.StopAsync(CT);

        await using var guest2Conn = await ConnectSignalRGuestAsync(MatchmakingHubPath, "guest2");
        await guest2Conn.InvokeAsync(SeekCasualMethod, timeControl, CT);

        await AssertMatchEstablishedAsync(guest1ActiveConn, guest2Conn);
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
        conn.On<string>("MatchFoundAsync", gameId => tcs.TrySetResult(gameId));
        return tcs;
    }
}
