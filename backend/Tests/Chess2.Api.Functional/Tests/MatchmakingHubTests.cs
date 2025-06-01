using Chess2.Api.Infrastructure.SignalR;
using Chess2.Api.TestInfrastructure;
using Chess2.Api.TestInfrastructure.Fakes;
using Chess2.Api.TestInfrastructure.Utils;
using Chess2.Api.Users.Entities;
using FluentAssertions;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;

namespace Chess2.Api.Functional.Tests;

public class MatchmakingHubTests(Chess2WebApplicationFactory factory) : BaseFunctionalTest(factory)
{
    private const string HubPath = "/api/hub/matchmaking";
    private const string SeekCasualMethod = "SeekCasualAsync";
    private const string SeekRatedMethod = "SeekRatedAsync";

    [Fact]
    public async Task Connecting_without_access_token_throws_error()
    {
        var act = async () => await CreateSignalRConnectionAsync(HubPath);
        await act.Should().ThrowAsync<HttpRequestException>().WithMessage("*Unauthorized*");
    }

    [Fact]
    public async Task SeekCasualAsync_guest_vs_guest_matches()
    {
        await using var conn1 = await ConnectGuestAsync("guest1");
        await using var conn2 = await ConnectGuestAsync("guest2");

        await AssertPlayersMatchAsync(conn1, conn2, SeekCasualMethod, 10, 0);
    }

    [Fact]
    public async Task SeekRatedAsync_match_with_two_authed_users()
    {
        var user1 = await FakerUtils.StoreFakerAsync(DbContext, new AuthedUserFaker());
        var user2 = await FakerUtils.StoreFakerAsync(DbContext, new AuthedUserFaker());

        await using var conn1 = await ConnectAuthedAsync(user1);
        await using var conn2 = await ConnectAuthedAsync(user2);

        await AssertPlayersMatchAsync(conn1, conn2, SeekRatedMethod, 5, 10);
    }

    [Fact]
    public async Task SeekCasualAsync_match_with_authed_and_guest()
    {
        var authedUser = await FakerUtils.StoreFakerAsync(DbContext, new AuthedUserFaker());

        await using var conn1 = await ConnectAuthedAsync(authedUser);
        await using var conn2 = await ConnectGuestAsync("guest1");

        await AssertPlayersMatchAsync(conn1, conn2, SeekCasualMethod, 15, 3);
    }

    [Fact]
    public async Task SeekRatedAsync_and_SeekCasualAsync_with_multiple_concurrent_user_pairs()
    {
        var user1Match1 = await FakerUtils.StoreFakerAsync(DbContext, new AuthedUserFaker());
        var user2Match1 = await FakerUtils.StoreFakerAsync(DbContext, new AuthedUserFaker());

        var user1Match2 = await FakerUtils.StoreFakerAsync(DbContext, new AuthedUserFaker());
        var user2Match2 = await FakerUtils.StoreFakerAsync(DbContext, new AuthedUserFaker());

        await using var conn1Match1 = await ConnectAuthedAsync(user1Match1);
        await using var conn2Match1 = await ConnectAuthedAsync(user2Match1);

        await using var conn1Match2 = await ConnectAuthedAsync(user1Match2);
        await using var conn2Match2 = await ConnectAuthedAsync(user2Match2);

        await using var conn1Match3 = await ConnectGuestAsync("guest1");
        await using var conn2Match3 = await ConnectGuestAsync("guest2");

        var match1AssertTask = AssertPlayersMatchAsync(
            conn1Match1,
            conn2Match1,
            SeekRatedMethod,
            10,
            5
        );
        var match2AssertTask = AssertPlayersMatchAsync(
            conn1Match2,
            conn2Match2,
            SeekRatedMethod,
            5,
            3
        );
        var match3AssertTask = AssertPlayersMatchAsync(
            conn1Match3,
            conn2Match3,
            SeekCasualMethod,
            5,
            3
        );

        await Task.WhenAll(match1AssertTask, match2AssertTask, match3AssertTask);
    }

    [Fact]
    public async Task SeekRated_with_a_guest_should_return_an_error()
    {
        var conn = await ConnectGuestAsync("guest1");

        var tsc = new TaskCompletionSource<IEnumerable<SignalRError>>();
        conn.On<IEnumerable<SignalRError>>("ReceiveErrorAsync", errors => tsc.TrySetResult(errors));

        await conn.InvokeAsync(SeekRatedMethod, 5, 10);

        var result = await tsc.Task.WaitAsync(TimeSpan.FromSeconds(10));
        result.Should().ContainSingle().Which.Code.Should().Be("General.Unauthorized");
    }

    [Fact]
    public async Task Seek_and_disconnect_cancels_the_seek()
    {
        await using var conn1 = await ConnectGuestAsync("guest1");
        await conn1.InvokeAsync(SeekCasualMethod, 5, 10);
        await conn1.StopAsync();

        await using var conn2 = await ConnectGuestAsync("guest2");
        await using var conn3 = await ConnectGuestAsync("guest3");

        // users are matched in the order they connected, so if conn1 disconnects, conn2 and conn3 should match
        await AssertPlayersMatchAsync(conn2, conn3, SeekCasualMethod, 5, 10);
    }

    [Fact]
    public async Task Seek_and_disconnect_on_another_connection_doesnt_cancel_the_seek()
    {
        await using var guest1ActiveConn = await ConnectGuestAsync("guest1");
        await using var guest1DisconnectedConn = await ConnectGuestAsync("guest1");
        await guest1ActiveConn.InvokeAsync(SeekCasualMethod, 5, 10);
        await guest1DisconnectedConn.StopAsync();

        await using var guest2Conn = await ConnectGuestAsync("guest2");
        await guest2Conn.InvokeAsync(SeekCasualMethod, 5, 10);

        await AssertMatchEstablishedAsync(guest1ActiveConn, guest2Conn);
    }

    private static async Task AssertPlayersMatchAsync(
        HubConnection conn1,
        HubConnection conn2,
        string methodName,
        int baseMinutes,
        int increment
    )
    {
        await conn1.InvokeAsync(methodName, baseMinutes, increment);
        await conn2.InvokeAsync(methodName, baseMinutes, increment);

        await AssertMatchEstablishedAsync(conn1, conn2);
    }

    private static async Task AssertMatchEstablishedAsync(HubConnection conn1, HubConnection conn2)
    {
        var tcs1 = ListenForMatch(conn1);
        var tcs2 = ListenForMatch(conn2);

        var timeout = TimeSpan.FromSeconds(10);
        var gameId1 = await tcs1.Task.WaitAsync(timeout);
        var gameId2 = await tcs2.Task.WaitAsync(timeout);

        gameId1.Should().NotBeNullOrEmpty().And.HaveLength(16).And.Be(gameId2);
    }

    private async Task<HubConnection> ConnectGuestAsync(string guestId)
    {
        var token = TokenProvider.GenerateGuestToken(guestId);
        var conn = await CreateSignalRConnectionAsync(HubPath, token);
        return conn;
    }

    private async Task<HubConnection> ConnectAuthedAsync(AuthedUser user)
    {
        var token = TokenProvider.GenerateAccessToken(user);
        var conn = await CreateSignalRConnectionAsync(HubPath, token);
        return conn;
    }

    private static TaskCompletionSource<string> ListenForMatch(HubConnection conn)
    {
        var tcs = new TaskCompletionSource<string>();
        conn.On<string>("MatchFoundAsync", gameId => tcs.TrySetResult(gameId));
        return tcs;
    }
}
