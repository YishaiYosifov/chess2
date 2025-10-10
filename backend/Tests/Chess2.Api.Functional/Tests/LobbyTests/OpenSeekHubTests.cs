using Chess2.Api.GameSnapshot.Models;
using Chess2.Api.Infrastructure.Sharding;
using Chess2.Api.Lobby.Grains;
using Chess2.Api.Matchmaking.Models;
using Chess2.Api.Profile.Models;
using Chess2.Api.Shared.Models;
using Chess2.Api.TestInfrastructure;
using Chess2.Api.TestInfrastructure.Fakes;
using Chess2.Api.TestInfrastructure.SignalRClients;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Chess2.Api.Functional.Tests.LobbyTests;

public class OpenSeekHubTests : BaseFunctionalTest
{
    private readonly IGrainFactory _grains;
    private readonly IShardRouter _shardRouter;
    private readonly int _openSeekShardCount;

    public OpenSeekHubTests(Chess2WebApplicationFactory factory)
        : base(factory)
    {
        var settings = Scope.ServiceProvider.GetRequiredService<IOptions<AppSettings>>();
        _openSeekShardCount = settings.Value.Lobby.OpenSeekShardCount;

        _shardRouter = Scope.ServiceProvider.GetRequiredService<IShardRouter>();
        _grains = Scope.ServiceProvider.GetRequiredService<IGrainFactory>();
    }

    [Fact]
    public async Task Guest_users_only_receives_casual_seeks()
    {
        var watcherId = UserId.Guest();
        await ClearShardForWatcher(watcherId);

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
            await GuestSignalRAsync(OpenSeekHubClient.Path, watcherId),
            CT
        );
        var seeks = await openSeek.GetNextOpenSeekBatchAsync(CT);

        seeks.Should().ContainSingle().Which.Pool.PoolType.Should().Be(PoolType.Casual);
    }

    [Fact]
    public async Task Authed_users_receive_rated_seeks()
    {
        var authedSeeker = new AuthedUserFaker().Generate();
        var authedWatcher = new AuthedUserFaker().Generate();
        await DbContext.AddRangeAsync(authedWatcher, authedSeeker);
        await DbContext.SaveChangesAsync(CT);
        await ClearShardForWatcher(authedWatcher.Id);

        await using LobbyHubClient authLobby = new(
            await AuthedSignalRAsync(LobbyHubClient.Path, authedSeeker)
        );
        await authLobby.SeekRatedAsync(new(BaseSeconds: 300, IncrementSeconds: 3), CT);

        await using OpenSeekHubClient openSeek = await OpenSeekHubClient.CreateSubscribedAsync(
            await AuthedSignalRAsync(OpenSeekHubClient.Path, authedWatcher),
            CT
        );

        var seeks = await openSeek.GetNextOpenSeekBatchAsync(CT);
        seeks.Should().ContainSingle().Which.Pool.PoolType.Should().Be(PoolType.Rated);
    }

    [Fact]
    public async Task Authed_users_receive_casual_seeks()
    {
        var authedSeeker = new AuthedUserFaker().Generate();
        var authedWatcher = new AuthedUserFaker().Generate();
        await DbContext.AddRangeAsync(authedWatcher, authedSeeker);
        await DbContext.SaveChangesAsync(CT);
        await ClearShardForWatcher(authedWatcher.Id);

        await using LobbyHubClient casualLobby = new(
            await AuthedSignalRAsync(LobbyHubClient.Path, authedSeeker)
        );
        await casualLobby.SeekCasualAsync(new(BaseSeconds: 300, IncrementSeconds: 3), CT);

        await using OpenSeekHubClient openSeek = await OpenSeekHubClient.CreateSubscribedAsync(
            await AuthedSignalRAsync(OpenSeekHubClient.Path, authedWatcher),
            CT
        );

        var seeks = await openSeek.GetNextOpenSeekBatchAsync(CT);
        seeks.Should().ContainSingle().Which.Pool.PoolType.Should().Be(PoolType.Casual);
    }

    [Fact]
    public async Task Seek_end_notifies_subscribed_users()
    {
        var watcherId = UserId.Guest();
        await ClearShardForWatcher(watcherId);

        TimeControlSettings timeControl = new(BaseSeconds: 300, IncrementSeconds: 3);
        var seekerId = UserId.Guest();
        await using var lobby = new LobbyHubClient(
            await GuestSignalRAsync(LobbyHubClient.Path, seekerId)
        );
        await lobby.SeekCasualAsync(timeControl, CT);

        await using var watcher = await OpenSeekHubClient.CreateSubscribedAsync(
            await GuestSignalRAsync(OpenSeekHubClient.Path, watcherId),
            CT
        );
        // wait until the seek is broadcasted
        var seekCreated = await watcher.GetNextOpenSeekBatchAsync(CT);
        seekCreated.Should().ContainSingle().Which.UserId.Should().Be(seekerId);

        await lobby.CancelSeekAsync(new(PoolType.Casual, timeControl), CT);

        var (removedSeekerId, removedPool) = await watcher.GetNextOpenSeekRemovedAsync(CT);
        removedPool.PoolType.Should().Be(PoolType.Casual);
        removedSeekerId.Should().Be(seekerId);
    }

    [Fact]
    public async Task Multiple_connections_receive_same_notifications()
    {
        var watcherId1 = UserId.Guest();
        var watcherId2 = UserId.Guest();
        await ClearShardForWatcher(watcherId1, watcherId2);

        await using var watcher1 = await OpenSeekHubClient.CreateSubscribedAsync(
            await GuestSignalRAsync(OpenSeekHubClient.Path, watcherId1),
            CT
        );
        await using var watcher2 = await OpenSeekHubClient.CreateSubscribedAsync(
            await GuestSignalRAsync(OpenSeekHubClient.Path, watcherId2),
            CT
        );

        await using var lobby = new LobbyHubClient(await GuestSignalRAsync(LobbyHubClient.Path));
        await lobby.SeekCasualAsync(new(BaseSeconds: 300, IncrementSeconds: 3), CT);

        var watcher1Seeks = await watcher1.GetNextOpenSeekBatchAsync(CT);
        var watcher2Seeks = await watcher2.GetNextOpenSeekBatchAsync(CT);
        watcher1Seeks.Count.Should().Be(1);
        watcher2Seeks.Should().BeEquivalentTo(watcher1Seeks);
    }

    private async Task ClearShardForWatcher(params UserId[] userIds)
    {
        foreach (var userId in userIds)
        {
            var shard = _shardRouter.GetShardNumber(userId, _openSeekShardCount);
            await _grains.GetGrain<IOpenSeekGrain>(shard).ClearStateAsync();
        }
    }
}
