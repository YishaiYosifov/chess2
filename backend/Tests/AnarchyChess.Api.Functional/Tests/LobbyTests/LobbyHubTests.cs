using AnarchyChess.Api.Game.Models;
using AnarchyChess.Api.GameSnapshot.Models;
using AnarchyChess.Api.Matchmaking.Models;
using AnarchyChess.Api.Profile.Models;
using AnarchyChess.Api.TestInfrastructure;
using AnarchyChess.Api.TestInfrastructure.Fakes;
using AnarchyChess.Api.TestInfrastructure.SignalRClients;
using AwesomeAssertions;

namespace AnarchyChess.Api.Functional.Tests.LobbyTests;

public class LobbyHubTests(AnarchyChessWebApplicationFactory factory) : BaseFunctionalTest(factory)
{
    [Fact]
    public async Task SeekRatedAsync_with_invalid_time_control_returns_an_error()
    {
        var user = new AuthedUserFaker().Generate();
        await DbContext.AddAsync(user, CT);
        await DbContext.SaveChangesAsync(CT);

        await using LobbyHubClient conn = new(AuthedSignalR(LobbyHubClient.Path, user));
        await conn.StartAsync(CT);

        await conn.SeekRatedAsync(new TimeControlSettings(4912, -5), CT);
        var errors = await conn.GetNextErrorsAsync(CT);

        errors.Should().HaveCountGreaterThanOrEqualTo(1);
        errors.Should().AllSatisfy(x => x.Code.Should().Be("General.Validation"));
    }

    [Fact]
    public async Task SeekCasualAsync_with_invalid_time_control_returns_an_error()
    {
        await using LobbyHubClient conn = new(GuestSignalR(LobbyHubClient.Path));
        await conn.StartAsync(CT);

        await conn.SeekCasualAsync(new TimeControlSettings(56, 531), CT);
        var errors = await conn.GetNextErrorsAsync(CT);

        errors.Should().HaveCountGreaterThanOrEqualTo(1);
        errors.Should().AllSatisfy(x => x.Code.Should().Be("General.Validation"));
    }

    [Fact]
    public async Task SeekCasualAsync_guest_vs_guest_matches()
    {
        await using LobbyHubClient conn1 = new(GuestSignalR(LobbyHubClient.Path));
        await using LobbyHubClient conn2 = new(GuestSignalR(LobbyHubClient.Path));
        await conn1.StartAsync(CT);
        await conn2.StartAsync(CT);

        await conn1.SeekCasualAsync(new TimeControlSettings(600, 0), CT);
        await conn2.SeekCasualAsync(new TimeControlSettings(600, 0), CT);

        await AssertMatchEstablishedAsync(conn1, conn2);
    }

    [Fact]
    public async Task SeekRatedAsync_match_with_two_authed_users()
    {
        var user1 = new AuthedUserFaker().Generate();
        var user2 = new AuthedUserFaker().Generate();
        await DbContext.AddRangeAsync(user1, user2);
        await DbContext.SaveChangesAsync(CT);

        await using LobbyHubClient conn1 = new(AuthedSignalR(LobbyHubClient.Path, user1));
        await using LobbyHubClient conn2 = new(AuthedSignalR(LobbyHubClient.Path, user2));
        await conn1.StartAsync(CT);
        await conn2.StartAsync(CT);

        await conn1.SeekRatedAsync(new TimeControlSettings(300, 10), CT);
        await conn2.SeekRatedAsync(new TimeControlSettings(300, 10), CT);

        await AssertMatchEstablishedAsync(conn1, conn2);
    }

    [Fact]
    public async Task SeekCasualAsync_match_with_authed_and_guest()
    {
        var authedUser = new AuthedUserFaker().Generate();
        await DbContext.AddAsync(authedUser, CT);
        await DbContext.SaveChangesAsync(CT);

        await using LobbyHubClient conn1 = new(AuthedSignalR(LobbyHubClient.Path, authedUser));
        await using LobbyHubClient conn2 = new(GuestSignalR(LobbyHubClient.Path));
        await conn1.StartAsync(CT);
        await conn2.StartAsync(CT);

        await conn1.SeekCasualAsync(new TimeControlSettings(900, 3), CT);
        await conn2.SeekCasualAsync(new TimeControlSettings(900, 3), CT);

        await AssertMatchEstablishedAsync(conn1, conn2);
    }

    [Fact]
    public async Task SeekRatedAsync_and_SeekCasualAsync_with_multiple_concurrent_user_pairs()
    {
        TimeControlSettings timeControl = new(600, 5);

        var authed1 = new AuthedUserFaker().Generate();
        var authed2 = new AuthedUserFaker().Generate();
        var authed3 = new AuthedUserFaker().Generate();
        var authed4 = new AuthedUserFaker().Generate();
        await DbContext.AddRangeAsync(authed1, authed2, authed3, authed4);
        await DbContext.SaveChangesAsync(CT);

        await using LobbyHubClient authedConn1 = new(AuthedSignalR(LobbyHubClient.Path, authed1));
        await using LobbyHubClient authedConn2 = new(AuthedSignalR(LobbyHubClient.Path, authed2));
        await using LobbyHubClient authedConn3 = new(AuthedSignalR(LobbyHubClient.Path, authed3));
        await using LobbyHubClient authedConn4 = new(AuthedSignalR(LobbyHubClient.Path, authed4));
        await using LobbyHubClient guestConn1 = new(GuestSignalR(LobbyHubClient.Path));
        await using LobbyHubClient guestConn2 = new(GuestSignalR(LobbyHubClient.Path));

        await authedConn1.StartAsync(CT);
        await authedConn2.StartAsync(CT);
        await authedConn3.StartAsync(CT);
        await authedConn4.StartAsync(CT);
        await guestConn1.StartAsync(CT);
        await guestConn2.StartAsync(CT);

        List<Task> connTasks =
        [
            authedConn1.SeekRatedAsync(timeControl, CT),
            authedConn2.SeekRatedAsync(timeControl, CT),
            authedConn3.SeekRatedAsync(timeControl, CT),
            authedConn4.SeekRatedAsync(timeControl, CT),
            guestConn1.SeekCasualAsync(timeControl, CT),
            guestConn2.SeekCasualAsync(timeControl, CT),
        ];

        var concurrentRatedMatchTask = AssertConcurrentMatchesAsync(
            authedConn1,
            authedConn2,
            authedConn3,
            authedConn4
        );
        var guestMatchTask = AssertMatchEstablishedAsync(guestConn1, guestConn2);

        await Task.WhenAll([.. connTasks, concurrentRatedMatchTask, guestMatchTask]);
    }

    [Fact]
    public async Task SeekRated_with_a_guest_should_return_an_error()
    {
        await using LobbyHubClient conn = new(GuestSignalR(LobbyHubClient.Path));
        await conn.StartAsync(CT);

        await conn.SeekRatedAsync(new TimeControlSettings(300, 10), CT);

        var result = await conn.GetNextErrorsAsync(CT);

        result.Should().ContainSingle().Which.Code.Should().Be("General.Unauthorized");
    }

    [Fact]
    public async Task Seek_and_disconnect_cancels_the_seek()
    {
        TimeControlSettings timeControl = new(300, 10);

        await using LobbyHubClient conn1 = new(GuestSignalR(LobbyHubClient.Path));
        await conn1.StartAsync(CT);
        await conn1.SeekCasualAsync(timeControl, CT);
        await conn1.DisposeAsync();

        await using LobbyHubClient conn2 = new(GuestSignalR(LobbyHubClient.Path));
        await using LobbyHubClient conn3 = new(GuestSignalR(LobbyHubClient.Path));
        await conn2.StartAsync(CT);
        await conn3.StartAsync(CT);

        await conn2.SeekCasualAsync(timeControl, CT);
        await conn3.SeekCasualAsync(timeControl, CT);

        await AssertMatchEstablishedAsync(conn2, conn3);
    }

    [Fact]
    public async Task Seek_and_disconnect_on_another_connection_doesnt_cancel_the_seek()
    {
        TimeControlSettings timeControl = new(300, 10);

        await using LobbyHubClient guest1ActiveConn = new(GuestSignalR(LobbyHubClient.Path));
        LobbyHubClient guest1DisconnectedConn = new(GuestSignalR(LobbyHubClient.Path));
        await guest1ActiveConn.StartAsync(CT);
        await guest1DisconnectedConn.StartAsync(CT);

        await guest1ActiveConn.SeekCasualAsync(timeControl, CT);
        await guest1DisconnectedConn.StopAsync(CT);

        await using LobbyHubClient guest2Conn = new(GuestSignalR(LobbyHubClient.Path));
        await guest2Conn.StartAsync(CT);
        await guest2Conn.SeekCasualAsync(timeControl, CT);

        await AssertMatchEstablishedAsync(guest1ActiveConn, guest2Conn);
    }

    [Fact]
    public async Task MatchWithOpenSeekAsync_matches_user_with_open_seek()
    {
        TimeControlSettings timeControl = new(300, 10);

        var seekerId = UserId.Guest();
        await using LobbyHubClient conn1 = new(GuestSignalR(LobbyHubClient.Path, seekerId));
        await using LobbyHubClient conn2 = new(GuestSignalR(LobbyHubClient.Path));
        await conn1.StartAsync(CT);
        await conn2.StartAsync(CT);

        await conn1.SeekCasualAsync(timeControl, CT);
        await conn2.MatchWithOpenSeekAsync(
            matchWith: seekerId,
            new PoolKey(PoolType.Casual, timeControl),
            CT
        );

        await AssertMatchEstablishedAsync(conn1, conn2);
    }

    [Fact]
    public async Task OnConnectedAsync_sends_ongoing_games_to_client()
    {
        TimeControlSettings timeControl = new(300, 10);

        var seekerId = UserId.Guest();
        await using LobbyHubClient conn1 = new(GuestSignalR(LobbyHubClient.Path, seekerId));
        await using LobbyHubClient conn2 = new(GuestSignalR(LobbyHubClient.Path));
        await conn1.StartAsync(CT);
        await conn2.StartAsync(CT);

        await conn1.SeekCasualAsync(timeControl, CT);
        await conn2.MatchWithOpenSeekAsync(
            matchWith: seekerId,
            new PoolKey(PoolType.Casual, timeControl),
            CT
        );

        GameToken gameToken = await AssertMatchEstablishedAsync(conn1, conn2);

        var connectedOngoing = await conn1.GetNextOngoingGamesBatchAsync(CT);
        connectedOngoing.Should().ContainSingle().Which.GameToken.Should().Be(gameToken);

        await using LobbyHubClient conn1Reconnect = new(
            GuestSignalR(LobbyHubClient.Path, seekerId)
        );
        await conn1Reconnect.StartAsync(CT);

        var reconnectOngoing = await conn1Reconnect.GetNextOngoingGamesBatchAsync(CT);
        reconnectOngoing.Should().ContainSingle().Which.GameToken.Should().Be(gameToken);
    }

    private async Task AssertConcurrentMatchesAsync(params List<LobbyHubClient> conns)
    {
        var tokens = await Task.WhenAll(conns.Select(conn => conn.WaitForGameAsync(CT)));
        var tokenCounts = tokens.GroupBy(token => token).ToDictionary(g => g.Key, g => g.Count());

        tokenCounts.Should().HaveCount(conns.Count / 2);
        foreach (var kvp in tokenCounts)
        {
            kvp.Value.Should().Be(2);
            kvp.Key.Should().NotBeNullOrEmpty().And.HaveLength(16);
        }
    }

    private async Task<string> AssertMatchEstablishedAsync(
        LobbyHubClient conn1,
        LobbyHubClient conn2
    )
    {
        var gameToken1 = await conn1.WaitForGameAsync(CT);
        var gameToken2 = await conn2.WaitForGameAsync(CT);

        gameToken1.Should().NotBeNullOrEmpty().And.HaveLength(16).And.Be(gameToken2);

        return gameToken1;
    }
}
