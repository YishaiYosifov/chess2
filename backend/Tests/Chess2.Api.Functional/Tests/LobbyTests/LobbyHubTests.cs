using Chess2.Api.GameSnapshot.Models;
using Chess2.Api.Matchmaking.Models;
using Chess2.Api.Profile.Models;
using Chess2.Api.TestInfrastructure;
using Chess2.Api.TestInfrastructure.Fakes;
using Chess2.Api.TestInfrastructure.SignalRClients;
using FluentAssertions;

namespace Chess2.Api.Functional.Tests.LobbyTests;

public class LobbyHubTests(Chess2WebApplicationFactory factory) : BaseFunctionalTest(factory)
{
    [Fact]
    public async Task Connecting_without_access_token_throws_error()
    {
        var act = async () => await SignalRAsync(LobbyHubClient.Path);
        await act.Should().ThrowAsync<HttpRequestException>().WithMessage("*Unauthorized*");
    }

    [Fact]
    public async Task SeekCasualAsync_guest_vs_guest_matches()
    {
        await using LobbyHubClient conn1 = new(await GuestSignalRAsync(LobbyHubClient.Path));
        await using LobbyHubClient conn2 = new(await GuestSignalRAsync(LobbyHubClient.Path));

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

        await using LobbyHubClient conn1 = new(
            await AuthedSignalRAsync(LobbyHubClient.Path, user1)
        );
        await using LobbyHubClient conn2 = new(
            await AuthedSignalRAsync(LobbyHubClient.Path, user2)
        );

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

        await using LobbyHubClient conn1 = new(
            await AuthedSignalRAsync(LobbyHubClient.Path, authedUser)
        );
        await using LobbyHubClient conn2 = new(await GuestSignalRAsync(LobbyHubClient.Path));

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

        await using LobbyHubClient authedConn1 = new(
            await AuthedSignalRAsync(LobbyHubClient.Path, authed1)
        );
        await using LobbyHubClient authedConn2 = new(
            await AuthedSignalRAsync(LobbyHubClient.Path, authed2)
        );
        await using LobbyHubClient authedConn3 = new(
            await AuthedSignalRAsync(LobbyHubClient.Path, authed3)
        );
        await using LobbyHubClient authedConn4 = new(
            await AuthedSignalRAsync(LobbyHubClient.Path, authed4)
        );

        await using LobbyHubClient guestConn1 = new(await GuestSignalRAsync(LobbyHubClient.Path));
        await using LobbyHubClient guestConn2 = new(await GuestSignalRAsync(LobbyHubClient.Path));

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
        await using LobbyHubClient conn = new(await GuestSignalRAsync(LobbyHubClient.Path));

        await conn.SeekRatedAsync(new TimeControlSettings(300, 10), CT);

        var result = await conn.WaitForErrorAsync(CT);

        result.Should().ContainSingle().Which.Code.Should().Be("General.Unauthorized");
    }

    [Fact]
    public async Task Seek_and_disconnect_cancels_the_seek()
    {
        TimeControlSettings timeControl = new(300, 10);

        await using LobbyHubClient conn1 = new(await GuestSignalRAsync(LobbyHubClient.Path));
        await conn1.SeekCasualAsync(timeControl, CT);
        await conn1.DisposeAsync();

        await using LobbyHubClient conn2 = new(await GuestSignalRAsync(LobbyHubClient.Path));
        await using LobbyHubClient conn3 = new(await GuestSignalRAsync(LobbyHubClient.Path));

        await conn2.SeekCasualAsync(timeControl, CT);
        await conn3.SeekCasualAsync(timeControl, CT);

        await AssertMatchEstablishedAsync(conn2, conn3);
    }

    [Fact]
    public async Task Seek_and_disconnect_on_another_connection_doesnt_cancel_the_seek()
    {
        TimeControlSettings timeControl = new(300, 10);

        await using LobbyHubClient guest1ActiveConn = new(
            await GuestSignalRAsync(LobbyHubClient.Path)
        );
        await using var guest1DisconnectedConn = await GuestSignalRAsync(LobbyHubClient.Path);
        await guest1ActiveConn.SeekCasualAsync(timeControl, CT);
        await guest1DisconnectedConn.StopAsync(CT);

        await using LobbyHubClient guest2Conn = new(await GuestSignalRAsync(LobbyHubClient.Path));
        await guest2Conn.SeekCasualAsync(timeControl, CT);

        await AssertMatchEstablishedAsync(guest1ActiveConn, guest2Conn);
    }

    [Fact]
    public async Task MatchWithOpenSeekAsync_matches_user_with_open_seek()
    {
        TimeControlSettings timeControl = new(300, 10);

        var seekerId = UserId.Guest();
        await using LobbyHubClient conn1 = new(
            await GuestSignalRAsync(LobbyHubClient.Path, seekerId)
        );
        await using LobbyHubClient conn2 = new(await GuestSignalRAsync(LobbyHubClient.Path));

        await conn1.SeekCasualAsync(timeControl, CT);
        await conn2.MatchWithOpenSeekAsync(
            matchWith: seekerId,
            new PoolKey(PoolType.Casual, timeControl),
            CT
        );

        await AssertMatchEstablishedAsync(conn1, conn2);
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
