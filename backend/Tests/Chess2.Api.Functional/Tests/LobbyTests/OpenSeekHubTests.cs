using Chess2.Api.GameSnapshot.Models;
using Chess2.Api.Matchmaking.Models;
using Chess2.Api.TestInfrastructure;
using Chess2.Api.TestInfrastructure.Fakes;
using Chess2.Api.TestInfrastructure.SignalRClients;
using FluentAssertions;

namespace Chess2.Api.Functional.Tests.LobbyTests;

public class OpenSeekHubTests(Chess2WebApplicationFactory factory) : BaseFunctionalTest(factory)
{
    //public override ValueTask DisposeAsync()
    //{
    //    Scope.ServiceProvider.GetRequired
    //    return base.DisposeAsync();
    //}

    [Fact]
    public async Task Guest_users_only_receives_casual_seeks()
    {
        var authedSeeker = new AuthedUserFaker().Generate();
        await DbContext.AddAsync(authedSeeker, CT);
        await DbContext.SaveChangesAsync(CT);

        await using LobbyHubClient authLobby = new(
            await AuthedSignalRAsync(LobbyHubClient.Path, authedSeeker)
        );
        await authLobby.SeekRatedAsync(new(BaseSeconds: 300, IncrementSeconds: 3), CT);

        await using LobbyHubClient casualLobby = new(
            await AuthedSignalRAsync(LobbyHubClient.Path, authedSeeker)
        );
        await casualLobby.SeekCasualAsync(new(BaseSeconds: 300, IncrementSeconds: 3), CT);

        await using OpenSeekHubClient openSeek = await OpenSeekHubClient.CreateSubscribedAsync(
            await GuestSignalRAsync(OpenSeekHubClient.Path),
            CT
        );
        var seeks = await openSeek.GetOpenSeekBatchesAsync(1, CT);

        seeks.Should().ContainSingle().Which.Pool.PoolType.Should().Be(PoolType.Casual);
    }

    [Fact]
    public async Task Authed_users_receive_rated_seeks()
    {
        var authedSeeker = new AuthedUserFaker().Generate();
        var authedWatcher = new AuthedUserFaker().Generate();
        await DbContext.AddRangeAsync(authedWatcher, authedSeeker);
        await DbContext.SaveChangesAsync(CT);

        await using LobbyHubClient authLobby = new(
            await AuthedSignalRAsync(LobbyHubClient.Path, authedSeeker)
        );
        await authLobby.SeekRatedAsync(new(BaseSeconds: 300, IncrementSeconds: 3), CT);

        await using OpenSeekHubClient openSeek = await OpenSeekHubClient.CreateSubscribedAsync(
            await AuthedSignalRAsync(OpenSeekHubClient.Path, authedWatcher),
            CT
        );

        var seeks = await openSeek.GetOpenSeekBatchesAsync(1, CT);
        seeks.Should().ContainSingle().Which.Pool.PoolType.Should().Be(PoolType.Rated);
    }

    [Fact]
    public async Task Authed_users_receive_casual_seeks()
    {
        var authedSeeker = new AuthedUserFaker().Generate();
        var authedWatcher = new AuthedUserFaker().Generate();
        await DbContext.AddRangeAsync(authedWatcher, authedSeeker);
        await DbContext.SaveChangesAsync(CT);

        await using LobbyHubClient casualLobby = new(
            await AuthedSignalRAsync(LobbyHubClient.Path, authedSeeker)
        );
        await casualLobby.SeekCasualAsync(new(BaseSeconds: 300, IncrementSeconds: 3), CT);

        await using OpenSeekHubClient openSeek = await OpenSeekHubClient.CreateSubscribedAsync(
            await AuthedSignalRAsync(OpenSeekHubClient.Path, authedWatcher),
            CT
        );

        var seeks = await openSeek.GetOpenSeekBatchesAsync(1, CT);
        seeks.Should().ContainSingle().Which.Pool.PoolType.Should().Be(PoolType.Casual);
    }

    [Fact]
    public async Task Seek_end_notifies_subscribed_users()
    {
        TimeControlSettings timeControl = new(BaseSeconds: 300, IncrementSeconds: 3);
        await using var lobby = new LobbyHubClient(await GuestSignalRAsync(LobbyHubClient.Path));
        await lobby.SeekCasualAsync(timeControl, CT);

        await using var watcher = await OpenSeekHubClient.CreateSubscribedAsync(
            await GuestSignalRAsync(OpenSeekHubClient.Path),
            CT
        );

        await lobby.CancelSeekAsync(new(PoolType.Casual, timeControl), CT);

        var seeks = await watcher.GetOpenSeekRemovedAsync(1, CT);
        seeks.Should().ContainSingle();
    }

    [Fact]
    public async Task Multiple_connections_receive_same_notifications()
    {
        await using var watcher1 = await OpenSeekHubClient.CreateSubscribedAsync(
            await GuestSignalRAsync(OpenSeekHubClient.Path),
            CT
        );
        await using var watcher2 = await OpenSeekHubClient.CreateSubscribedAsync(
            await GuestSignalRAsync(OpenSeekHubClient.Path),
            CT
        );

        await using var lobby = new LobbyHubClient(await GuestSignalRAsync(LobbyHubClient.Path));
        await lobby.SeekCasualAsync(new(BaseSeconds: 300, IncrementSeconds: 3), CT);

        var watcher1Seeks = await watcher1.GetOpenSeekBatchesAsync(1, CT);
        var watcher2Seeks = await watcher2.GetOpenSeekBatchesAsync(1, CT);
        watcher1Seeks.Count.Should().Be(1);
        watcher2Seeks.Should().BeEquivalentTo(watcher1Seeks);
    }
}
