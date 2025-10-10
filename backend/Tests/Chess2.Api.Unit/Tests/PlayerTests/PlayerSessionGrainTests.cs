using Chess2.Api.GameSnapshot.Models;
using Chess2.Api.Lobby.Errors;
using Chess2.Api.Lobby.Grains;
using Chess2.Api.Lobby.Services;
using Chess2.Api.Matchmaking.Errors;
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
using Orleans.TestKit.Storage;

namespace Chess2.Api.Unit.Tests.PlayerTests;

public class PlayerSessionGrainTests : BaseGrainTest
{
    private readonly ILobbyNotifier _matchmakingNotifierMock = Substitute.For<ILobbyNotifier>();
    private readonly LobbySettings _settings;

    private readonly IMatchmakingGrain<RatedMatchmakingPool> _ratedPoolGrainMock = Substitute.For<
        IMatchmakingGrain<RatedMatchmakingPool>
    >();
    private readonly IMatchmakingGrain<CasualMatchmakingPool> _casualPoolGrainMock = Substitute.For<
        IMatchmakingGrain<CasualMatchmakingPool>
    >();

    private const string UserId = "test-user-id";

    private readonly PlayerSessionState _state;
    private readonly TestStorageStats _stateStats;

    public PlayerSessionGrainTests()
    {
        Silo.AddProbe(_ => _ratedPoolGrainMock);
        Silo.AddProbe(_ => _casualPoolGrainMock);

        var settings = AppSettingsLoader.LoadAppSettings();
        _settings = settings.Lobby;

        Silo.ServiceProvider.AddService(Substitute.For<ILogger<PlayerSessionGrain>>());
        Silo.ServiceProvider.AddService(_matchmakingNotifierMock);
        Silo.ServiceProvider.AddService(Options.Create(settings));

        _state = Silo
            .StorageManager.GetStorage<PlayerSessionState>(PlayerSessionGrain.StateName)
            .State;
        _stateStats = Silo.StorageManager.GetStorageStats(PlayerSessionGrain.StateName)!;
    }

    [Fact]
    public async Task CreateSeekAsync_adds_seek_and_registers_connection()
    {
        var pool = new PoolKeyFaker(PoolType.Rated).Generate();
        var seeker = new RatedSeekerFaker(UserId).Generate();

        var grain = await Silo.CreateGrainAsync<PlayerSessionGrain>(UserId);
        var result = await grain.CreateSeekAsync("conn1", seeker, pool);

        result.IsError.Should().BeFalse();
        await _ratedPoolGrainMock.Received(1).AddSeekAsync(seeker, grain);

        _state.ConnectionMap.PoolConnections(pool).Should().BeEquivalentTo([(ConnectionId)"conn1"]);
        _stateStats.Writes.Should().BeGreaterThanOrEqualTo(1);
    }

    [Fact]
    public async Task CreateSeekAsync_allows_multiple_connections_to_same_pool()
    {
        var grain = await Silo.CreateGrainAsync<PlayerSessionGrain>(UserId);
        var pool = new PoolKeyFaker(PoolType.Casual).Generate();
        var seeker = new CasualSeekerFaker(UserId).Generate();

        (await grain.CreateSeekAsync("conn1", seeker, pool)).IsError.Should().BeFalse();
        (await grain.CreateSeekAsync("conn2", seeker, pool)).IsError.Should().BeFalse();

        await _casualPoolGrainMock.Received(2).AddSeekAsync(seeker, grain);
        _stateStats.Writes.Should().BeGreaterThanOrEqualTo(1);
        _state
            .ConnectionMap.PoolConnections(pool)
            .Should()
            .BeEquivalentTo([(ConnectionId)"conn1", (ConnectionId)"conn2"]);
    }

    [Fact]
    public async Task CreateSeekAsync_rejects_when_too_many_active_games()
    {
        var seeker = new CasualSeekerFaker(UserId).Generate();
        var pool = new PoolKeyFaker().Generate();

        var grain = await Silo.CreateGrainAsync<PlayerSessionGrain>(UserId);
        await FillGameLimitAsync(grain, seeker, pool);

        var result = await grain.CreateSeekAsync("extra-conn", seeker, pool);

        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(PlayerSessionErrors.TooManyGames);
        _state.ConnectionMap.PoolConnections(pool).Should().BeEmpty();
    }

    [Fact]
    public async Task CreateSeekAsync_rejects_connection_already_in_game()
    {
        var pool = new PoolKeyFaker().Generate();
        var seeker = new RatedSeekerFaker(UserId).Generate();

        var grain = await Silo.CreateGrainAsync<PlayerSessionGrain>(UserId);
        await grain.CreateSeekAsync("conn1", seeker, pool);
        await grain.SeekMatchedAsync("game1", pool);

        var result = await grain.CreateSeekAsync("conn1", seeker, pool);

        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(PlayerSessionErrors.ConnectionInGame);
        _state.ConnectionMap.PoolConnections(pool).Should().BeEmpty();
    }

    [Fact]
    public async Task CleanupConnectionAsync_only_cancels_the_seek_when_the_connection_is_the_only_in_pool()
    {
        var poolToRemove = new PoolKeyFaker(PoolType.Casual).Generate();
        var poolStillActive = new PoolKeyFaker(PoolType.Rated).Generate();
        var seeker = new RatedSeekerFaker(UserId).Generate();

        var grain = await Silo.CreateGrainAsync<PlayerSessionGrain>(UserId);
        await grain.CreateSeekAsync("conn1", seeker, poolToRemove);
        await grain.CreateSeekAsync("conn2", seeker, poolStillActive);
        await grain.CreateSeekAsync("conn3", seeker, poolToRemove); // keeps poolToRemove active

        await grain.CleanupConnectionAsync("conn1");

        await _casualPoolGrainMock.DidNotReceiveWithAnyArgs().TryCancelSeekAsync(default!);
        await _ratedPoolGrainMock.DidNotReceiveWithAnyArgs().TryCancelSeekAsync(default!);
        _state
            .ConnectionMap.PoolConnections(poolStillActive)
            .Should()
            .BeEquivalentTo([(ConnectionId)"conn2"]);
        _state
            .ConnectionMap.PoolConnections(poolToRemove)
            .Should()
            .BeEquivalentTo([(ConnectionId)"conn3"]);
        _stateStats.Writes.Should().BeGreaterThanOrEqualTo(1);
    }

    [Fact]
    public async Task CleanupConnectionAsync_removes_connection_from_multiple_pools()
    {
        var pool1 = new PoolKeyFaker(PoolType.Casual).Generate();
        var pool2 = new PoolKeyFaker(PoolType.Rated).Generate();
        var seeker = new RatedSeekerFaker(UserId).Generate();

        var grain = await Silo.CreateGrainAsync<PlayerSessionGrain>(UserId);
        await grain.CreateSeekAsync("conn1", seeker, pool1);
        await grain.CreateSeekAsync("conn1", seeker, pool2);

        await grain.CleanupConnectionAsync("conn1");

        await _casualPoolGrainMock.Received(1).TryCancelSeekAsync(UserId);
        await _ratedPoolGrainMock.Received(1).TryCancelSeekAsync(UserId);
        _state.ConnectionMap.ActivePools.Should().BeEmpty();
        _stateStats.Writes.Should().BeGreaterThanOrEqualTo(1);
    }

    [Fact]
    public async Task CleanupConnectionAsync_removes_connection_from_recently_matched_and_allows_new_seek()
    {
        var pool = new PoolKeyFaker().Generate();
        var seeker = new CasualSeekerFaker(UserId).Generate();

        var grain = await Silo.CreateGrainAsync<PlayerSessionGrain>(UserId);

        await grain.CreateSeekAsync("conn1", seeker, pool);
        await grain.SeekMatchedAsync("game1", pool);

        // At this point, conn1 should be in _connectionsRecentlyMatched and blocked
        (await grain.CreateSeekAsync("conn1", seeker, pool))
            .IsError.Should()
            .BeTrue();

        await grain.CleanupConnectionAsync("conn1");
        var result = await grain.CreateSeekAsync("conn1", seeker, pool);
        result.IsError.Should().BeFalse();
    }

    [Fact]
    public async Task CancelSeekAsync_removes_pool_and_notifies_match_failed()
    {
        var pool = new PoolKeyFaker(PoolType.Rated).Generate();
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
        var poolToMatch = new PoolKeyFaker(PoolType.Rated).Generate();
        var anoterPool = new PoolKeyFaker(PoolType.Casual).Generate();
        var seeker = new RatedSeekerFaker(UserId).Generate();

        var grain = await Silo.CreateGrainAsync<PlayerSessionGrain>(UserId);
        await grain.CreateSeekAsync("conn1", seeker, poolToMatch);
        await grain.CreateSeekAsync("conn2", seeker, poolToMatch);
        await grain.CreateSeekAsync("conn3", seeker, anoterPool);

        var gameToken = "game-123";
        await grain.SeekMatchedAsync(gameToken, poolToMatch);

        List<ConnectionId> expectedConns = ["conn1", "conn2"];
        await _matchmakingNotifierMock
            .Received(1)
            .NotifyGameFoundAsync(
                Arg.Is<IEnumerable<ConnectionId>>(ids => ids.SequenceEqual(expectedConns)),
                gameToken
            );

        // after matching the pool should be removed
        (await grain.TryReserveSeekAsync(poolToMatch))
            .Should()
            .BeFalse();

        _state.ConnectionMap.ActivePools.Should().BeEquivalentTo([anoterPool]);
        _stateStats.Writes.Should().BeGreaterThanOrEqualTo(1);
    }

    [Fact]
    public async Task SeekRemovedAsync_notifies_match_failed()
    {
        var pool = new PoolKeyFaker().Generate();
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

        _state.ConnectionMap.ActivePools.Should().BeEmpty();
        _stateStats.Writes.Should().BeGreaterThanOrEqualTo(1);
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
        (await grain.TryReserveSeekAsync(pool)).Should().BeFalse();
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
        (await grain.TryReserveSeekAsync(pool2)).Should().BeFalse();
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
        (await grain.TryReserveSeekAsync(pool2)).Should().BeTrue(); // reserves conn2
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
        (await grain.TryReserveSeekAsync(pool)).Should().BeTrue();
    }

    [Fact]
    public async Task MatchWithOpenSeekAsync_starts_game_and_notifies()
    {
        var pool = new PoolKey(PoolType.Casual, new TimeControlSettings(60, 0));
        var grain = await Silo.CreateGrainAsync<PlayerSessionGrain>(UserId);

        var connection = "conn1";
        var seeker = new CasualSeekerFaker(UserId).Generate();
        var targetSeekerId = "target-user";
        var gameToken = "game123";

        _casualPoolGrainMock.MatchWithSeekerAsync(seeker, targetSeekerId).Returns(gameToken);

        var result = await grain.MatchWithOpenSeekAsync(connection, seeker, targetSeekerId, pool);

        result.IsError.Should().BeFalse();
        List<ConnectionId> expectedConns = [connection];
        await _casualPoolGrainMock.Received(1).MatchWithSeekerAsync(seeker, targetSeekerId);
        await _matchmakingNotifierMock
            .Received(1)
            .NotifyGameFoundAsync(
                Arg.Is<IEnumerable<ConnectionId>>(ids => ids.SequenceEqual(expectedConns)),
                gameToken
            );

        _state.ActiveGameTokens.Should().BeEquivalentTo([gameToken]);
        _stateStats.Writes.Should().BeGreaterThanOrEqualTo(1);
    }

    [Fact]
    public async Task MatchWithOpenSeekAsync_returns_error_if_game_limit_reached()
    {
        var pool = new PoolKey(PoolType.Casual, new TimeControlSettings(60, 0));
        var grain = await Silo.CreateGrainAsync<PlayerSessionGrain>(UserId);

        var seeker = new CasualSeekerFaker(UserId).Generate();
        await FillGameLimitAsync(grain, seeker, pool);

        var result = await grain.MatchWithOpenSeekAsync("connX", seeker, "any-user", pool);

        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(PlayerSessionErrors.TooManyGames);
        await _casualPoolGrainMock
            .DidNotReceiveWithAnyArgs()
            .MatchWithSeekerAsync(default!, default!);
    }

    [Fact]
    public async Task MatchWithOpenSeekAsync_returns_error_if_connection_taken()
    {
        var pool = new PoolKey(PoolType.Casual, new TimeControlSettings(60, 0));
        var grain = await Silo.CreateGrainAsync<PlayerSessionGrain>(UserId);

        var seeker = new CasualSeekerFaker(UserId).Generate();
        await grain.CreateSeekAsync("conn1", seeker, pool);
        await grain.SeekMatchedAsync("game1", pool); // now conn1 is in a game

        var result = await grain.MatchWithOpenSeekAsync("conn1", seeker, "any-user", pool);

        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(PlayerSessionErrors.ConnectionInGame);
        await _casualPoolGrainMock
            .DidNotReceiveWithAnyArgs()
            .MatchWithSeekerAsync(default!, default!);
    }

    [Fact]
    public async Task MatchWithOpenSeekAsync_returns_error_when_matchmaking_grain_returns_error()
    {
        var pool = new PoolKey(PoolType.Casual, new TimeControlSettings(60, 0));
        var grain = await Silo.CreateGrainAsync<PlayerSessionGrain>(UserId);

        var seeker = new CasualSeekerFaker(UserId).Generate();
        var targetSeekerId = "target-user";

        _casualPoolGrainMock
            .MatchWithSeekerAsync(seeker, targetSeekerId)
            .Returns(MatchmakingErrors.RequestedSeekerNotCompatible);

        var result = await grain.MatchWithOpenSeekAsync("conn1", seeker, targetSeekerId, pool);

        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(MatchmakingErrors.RequestedSeekerNotCompatible);
        await _matchmakingNotifierMock
            .DidNotReceiveWithAnyArgs()
            .NotifyGameFoundAsync(default!, default!);
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
