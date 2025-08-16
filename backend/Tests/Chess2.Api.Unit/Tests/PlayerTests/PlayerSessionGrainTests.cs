using Chess2.Api.GameSnapshot.Models;
using Chess2.Api.Lobby.Errors;
using Chess2.Api.Lobby.Grains;
using Chess2.Api.Lobby.Services;
using Chess2.Api.Matchmaking.Grains;
using Chess2.Api.Matchmaking.Models;
using Chess2.Api.Matchmaking.Services.Pools;
using Chess2.Api.Shared.Models;
using Chess2.Api.TestInfrastructure.Fakes;
using Chess2.Api.TestInfrastructure.Utils;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using Orleans.TestKit;

namespace Chess2.Api.Unit.Tests.PlayerTests;

public class PlayerSessionGrainTests : BaseGrainTest
{
    private readonly ILobbyNotifier _matchmakingNotifierMock = Substitute.For<ILobbyNotifier>();
    private readonly LobbySettings _settings;

    private readonly IMatchmakingGrain<IRatedMatchmakingPool> _ratedPoolGrainMock = Substitute.For<
        IMatchmakingGrain<IRatedMatchmakingPool>
    >();
    private readonly IMatchmakingGrain<ICasualMatchmakingPool> _casualPoolGrainMock =
        Substitute.For<IMatchmakingGrain<ICasualMatchmakingPool>>();

    private const string UserId = "test-user-id";

    public PlayerSessionGrainTests()
    {
        Silo.AddProbe(_ => _ratedPoolGrainMock);
        Silo.AddProbe(_ => _casualPoolGrainMock);

        var settings = AppSettingsLoader.LoadAppSettings();
        _settings = settings.Lobby;

        Silo.ServiceProvider.AddService(Substitute.For<ILogger<PlayerSessionGrain>>());
        Silo.ServiceProvider.AddService(_matchmakingNotifierMock);
        Silo.ServiceProvider.AddService(Options.Create(settings));
    }

    [Fact]
    public async Task CreateSeekAsync_adds_seek_and_registers_connection()
    {
        PoolKey pool = new(PoolType.Rated, new TimeControlSettings(300, 5));
        var seeker = new RatedSeekerFaker(UserId).Generate();

        var grain = await Silo.CreateGrainAsync<PlayerSessionGrain>(UserId);
        var result = await grain.CreateSeekAsync("conn1", seeker, pool);

        result.IsError.Should().BeFalse();
        await _ratedPoolGrainMock.Received(1).AddSeekAsync(seeker, grain);
    }

    [Fact]
    public async Task CreateSeekAsync_allows_multiple_connections_to_same_pool()
    {
        var grain = await Silo.CreateGrainAsync<PlayerSessionGrain>(UserId);
        PoolKey pool = new(PoolType.Casual, new TimeControlSettings(60, 0));
        var seeker = new CasualSeekerFaker(UserId).Generate();

        (await grain.CreateSeekAsync("conn1", seeker, pool)).IsError.Should().BeFalse();
        (await grain.CreateSeekAsync("conn2", seeker, pool)).IsError.Should().BeFalse();

        await _casualPoolGrainMock.Received(2).AddSeekAsync(seeker, grain);
    }

    [Fact]
    public async Task CreateSeekAsync_rejects_when_too_many_active_games()
    {
        var seeker = new CasualSeekerFaker(UserId).Generate();
        PoolKey pool = new(PoolType.Casual, new TimeControlSettings(60, 0));

        var grain = await Silo.CreateGrainAsync<PlayerSessionGrain>(UserId);
        await FillGameLimitAsync(grain, seeker, pool);

        var result = await grain.CreateSeekAsync("extra-conn", seeker, pool);
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(PlayerSessionErrors.TooManyGames);
    }

    [Fact]
    public async Task CreateSeekAsync_rejects_connection_already_in_game()
    {
        PoolKey pool = new(PoolType.Rated, new TimeControlSettings(300, 5));
        var seeker = new RatedSeekerFaker(UserId).Generate();

        var grain = await Silo.CreateGrainAsync<PlayerSessionGrain>(UserId);
        await grain.CreateSeekAsync("conn1", seeker, pool);
        await grain.SeekMatchedAsync("game1", pool);

        var result = await grain.CreateSeekAsync("conn1", seeker, pool);
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(PlayerSessionErrors.ConnectionInGame);
    }

    [Fact]
    public async Task CleanupConnectionAsync_removes_connection_and_cancels_empty_pools()
    {
        PoolKey poolToRemove = new(PoolType.Casual, new TimeControlSettings(300, 5));
        PoolKey poolStillActive = new(PoolType.Rated, new TimeControlSettings(300, 5));
        var seeker = new RatedSeekerFaker(UserId).Generate();

        var grain = await Silo.CreateGrainAsync<PlayerSessionGrain>(UserId);
        await grain.CreateSeekAsync("conn1", seeker, poolToRemove);
        await grain.CreateSeekAsync("conn2", seeker, poolStillActive);
        await grain.CreateSeekAsync("conn3", seeker, poolToRemove); // keeps poolToRemove active

        await grain.CleanupConnectionAsync("conn1");

        await _casualPoolGrainMock.DidNotReceive().TryCancelSeekAsync(UserId);
        await _ratedPoolGrainMock.DidNotReceive().TryCancelSeekAsync(UserId);
    }

    [Fact]
    public async Task CleanupConnectionAsync_removes_connection_from_multiple_pools()
    {
        PoolKey pool1 = new(PoolType.Casual, new TimeControlSettings(300, 5));
        PoolKey pool2 = new(PoolType.Rated, new TimeControlSettings(180, 2));
        var seeker = new RatedSeekerFaker(UserId).Generate();

        var grain = await Silo.CreateGrainAsync<PlayerSessionGrain>(UserId);
        await grain.CreateSeekAsync("conn1", seeker, pool1);
        await grain.CreateSeekAsync("conn1", seeker, pool2);

        await grain.CleanupConnectionAsync("conn1");

        await _casualPoolGrainMock.Received(1).TryCancelSeekAsync(UserId);
        await _ratedPoolGrainMock.Received(1).TryCancelSeekAsync(UserId);
    }

    [Fact]
    public async Task CancelSeekAsync_removes_pool_and_notifies_match_failed()
    {
        PoolKey pool = new(PoolType.Rated, new TimeControlSettings(300, 5));
        var seeker = new RatedSeekerFaker(UserId).Generate();

        var grain = await Silo.CreateGrainAsync<PlayerSessionGrain>(UserId);
        await grain.CreateSeekAsync("conn1", seeker, pool);
        await grain.CreateSeekAsync("conn2", seeker, pool);

        await grain.CancelSeekAsync(pool);
        await grain.SeekRemovedAsync(pool);

        List<ConnectionId> expectedConns = ["conn1", "conn2"];
        await _ratedPoolGrainMock.Received(1).TryCancelSeekAsync(UserId);
        await _matchmakingNotifierMock
            .Received(1)
            .NotifySeekFailedAsync(
                Arg.Is<IEnumerable<ConnectionId>>(ids => ids.SequenceEqual(expectedConns)),
                pool
            );
    }

    [Fact]
    public async Task SeekMatchedAsync_notifies_game_found_and_cleans_up_connections()
    {
        PoolKey pool = new(PoolType.Rated, new TimeControlSettings(180, 2));
        var seeker = new RatedSeekerFaker(UserId).Generate();

        var grain = await Silo.CreateGrainAsync<PlayerSessionGrain>(UserId);
        await grain.CreateSeekAsync("conn1", seeker, pool);
        await grain.CreateSeekAsync("conn2", seeker, pool);

        var gameToken = "game-123";
        await grain.SeekMatchedAsync(gameToken, pool);

        List<ConnectionId> expectedConns = ["conn1", "conn2"];
        await _matchmakingNotifierMock
            .Received(1)
            .NotifyGameFoundAsync(
                Arg.Is<IEnumerable<ConnectionId>>(ids => ids.SequenceEqual(expectedConns)),
                gameToken
            );

        // After matching, the pool should be removed
        (await grain.TryReserveSeekAsync(pool))
            .Should()
            .BeFalse();
    }

    [Fact]
    public async Task SeekRemovedAsync_notifies_match_failed()
    {
        PoolKey pool = new(PoolType.Casual, new TimeControlSettings(10, 1));
        var seeker = new CasualSeekerFaker(UserId).Generate();

        var grain = await Silo.CreateGrainAsync<PlayerSessionGrain>(UserId);
        await grain.CreateSeekAsync("conn1", seeker, pool);
        await grain.CreateSeekAsync("conn2", seeker, pool);

        await grain.SeekRemovedAsync(pool);

        List<ConnectionId> expectedConns = ["conn1", "conn2"];
        await _matchmakingNotifierMock
            .Received(1)
            .NotifySeekFailedAsync(
                Arg.Is<IEnumerable<ConnectionId>>(ids => ids.SequenceEqual(expectedConns)),
                pool
            );
    }

    [Fact]
    public async Task TryReserveSeekAsync_returns_false_if_game_limit_reached()
    {
        PoolKey pool = new(PoolType.Rated, new TimeControlSettings(180, 2));
        var seeker = new RatedSeekerFaker(UserId).Generate();

        var grain = await Silo.CreateGrainAsync<PlayerSessionGrain>(UserId);
        await FillGameLimitAsync(grain, seeker, pool);

        (await grain.TryReserveSeekAsync(pool)).Should().BeFalse();
    }

    [Fact]
    public async Task TryReserveSeekAsync_returns_false_if_pool_already_reserved()
    {
        PoolKey pool = new(PoolType.Casual, new TimeControlSettings(60, 0));
        var seeker = new CasualSeekerFaker(UserId).Generate();

        var grain = await Silo.CreateGrainAsync<PlayerSessionGrain>(UserId);
        await grain.CreateSeekAsync("conn1", seeker, pool);

        (await grain.TryReserveSeekAsync(pool)).Should().BeTrue();
        (await grain.TryReserveSeekAsync(pool)).Should().BeFalse(); // same pool again
    }

    [Fact]
    public async Task TryReserveSeekAsync_returns_false_if_all_connections_claimed_by_other_pools()
    {
        PoolKey pool1 = new(PoolType.Casual, new TimeControlSettings(60, 0));
        PoolKey pool2 = new(PoolType.Rated, new TimeControlSettings(300, 5));
        var seeker = new RatedSeekerFaker(UserId).Generate();

        var grain = await Silo.CreateGrainAsync<PlayerSessionGrain>(UserId);
        // same connection shared across both pools
        await grain.CreateSeekAsync("conn1", seeker, pool1);
        await grain.CreateSeekAsync("conn1", seeker, pool2);

        (await grain.TryReserveSeekAsync(pool1)).Should().BeTrue();
        (await grain.TryReserveSeekAsync(pool2)).Should().BeFalse(); // conn1 already claimed
    }

    [Fact]
    public async Task TryReserveSeekAsync_returns_true_when_reservable_connection_exists()
    {
        PoolKey pool1 = new(PoolType.Casual, new TimeControlSettings(60, 0));
        PoolKey pool2 = new(PoolType.Rated, new TimeControlSettings(300, 5));
        var seeker = new RatedSeekerFaker(UserId).Generate();

        var grain = await Silo.CreateGrainAsync<PlayerSessionGrain>(UserId);
        await grain.CreateSeekAsync("conn1", seeker, pool1);
        await grain.CreateSeekAsync("conn2", seeker, pool2);

        (await grain.TryReserveSeekAsync(pool1)).Should().BeTrue(); // reserves conn1
        (await grain.TryReserveSeekAsync(pool2)).Should().BeTrue(); // can still reserve conn2
    }

    [Fact]
    public async Task ReleaseReservationAsync_removes_claim()
    {
        PoolKey pool = new(PoolType.Rated, new TimeControlSettings(180, 2));
        var seeker = new RatedSeekerFaker(UserId).Generate();

        var grain = await Silo.CreateGrainAsync<PlayerSessionGrain>(UserId);
        await grain.CreateSeekAsync("conn1", seeker, pool);

        (await grain.TryReserveSeekAsync(pool)).Should().BeTrue();
        await grain.ReleaseReservationAsync(pool);
        (await grain.TryReserveSeekAsync(pool)).Should().BeTrue(); // Can reserve again
    }

    private async Task FillGameLimitAsync(PlayerSessionGrain grain, Seeker seeker, PoolKey pool)
    {
        for (var i = 0; i < _settings.MaxActiveGames; i++)
        {
            await grain.CreateSeekAsync($"conn{i}", seeker, pool);
            await grain.SeekMatchedAsync($"game{i}", pool);
        }
    }
}
