using AnarchyChess.Api.Game.Models;
using AnarchyChess.Api.GameSnapshot.Models;
using AnarchyChess.Api.Infrastructure;
using AnarchyChess.Api.Lobby.Errors;
using AnarchyChess.Api.Lobby.Grains;
using AnarchyChess.Api.Lobby.Services;
using AnarchyChess.Api.Matchmaking.Errors;
using AnarchyChess.Api.Matchmaking.Grains;
using AnarchyChess.Api.Matchmaking.Models;
using AnarchyChess.Api.Matchmaking.Services.Pools;
using AnarchyChess.Api.Profile.Models;
using AnarchyChess.Api.Shared.Models;
using AnarchyChess.Api.TestInfrastructure.Fakes;
using AnarchyChess.Api.TestInfrastructure.Utils;
using AwesomeAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using Orleans.TestKit;
using Orleans.TestKit.Storage;
using Orleans.TestKit.Streams;

namespace AnarchyChess.Api.Unit.Tests.PlayerTests;

public class PlayerSessionGrainTests : BaseGrainTest
{
    private readonly ILobbyNotifier _lobbyNotifier = Substitute.For<ILobbyNotifier>();
    private readonly LobbySettings _settings;

    private readonly IMatchmakingGrain<RatedMatchmakingPool> _ratedPoolGrainMock = Substitute.For<
        IMatchmakingGrain<RatedMatchmakingPool>
    >();
    private readonly IMatchmakingGrain<CasualMatchmakingPool> _casualPoolGrainMock = Substitute.For<
        IMatchmakingGrain<CasualMatchmakingPool>
    >();

    private readonly UserId _userId = "test-user-id";

    private readonly PlayerSessionState _state;
    private readonly TestStorageStats _stateStats;

    public PlayerSessionGrainTests()
    {
        Silo.AddProbe(_ => _ratedPoolGrainMock);
        Silo.AddProbe(_ => _casualPoolGrainMock);

        var settings = AppSettingsLoader.LoadAppSettings();
        _settings = settings.Lobby;

        Silo.ServiceProvider.AddService(Substitute.For<ILogger<PlayerSessionGrain>>());
        Silo.ServiceProvider.AddService(_lobbyNotifier);
        Silo.ServiceProvider.AddService(Options.Create(settings));

        _state = Silo
            .StorageManager.GetStorage<PlayerSessionState>(PlayerSessionGrain.StateName)
            .State;
        _stateStats = Silo.StorageManager.GetStorageStats(PlayerSessionGrain.StateName)!;
    }

    private TestStream<GameEndedEvent> ProbeGameEndedStream() =>
        Silo.AddStreamProbe<GameEndedEvent>(
            _userId,
            streamNamespace: nameof(GameEndedEvent),
            Streaming.StreamProvider
        );

    [Fact]
    public async Task CreateSeekAsync_adds_seek_and_registers_connection()
    {
        var pool = new PoolKeyFaker(PoolType.Rated).Generate();
        var seeker = new RatedSeekerFaker(_userId).Generate();

        var grain = await Silo.CreateGrainAsync<PlayerSessionGrain>(_userId);
        var result = await grain.CreateSeekAsync("conn1", seeker, pool, CT);

        result.IsError.Should().BeFalse();
        await _ratedPoolGrainMock.Received(1).AddSeekAsync(seeker, grain, CT);

        _state.ConnectionMap.PoolConnections(pool).Should().BeEquivalentTo([(ConnectionId)"conn1"]);
        _stateStats.Writes.Should().BeGreaterThanOrEqualTo(1);
        _stateStats.Clears.Should().Be(0);
    }

    [Fact]
    public async Task CreateSeekAsync_allows_multiple_connections_to_same_pool()
    {
        var grain = await Silo.CreateGrainAsync<PlayerSessionGrain>(_userId);
        var pool = new PoolKeyFaker(PoolType.Casual).Generate();
        var seeker = new CasualSeekerFaker(_userId).Generate();

        (await grain.CreateSeekAsync("conn1", seeker, pool, CT)).IsError.Should().BeFalse();
        (await grain.CreateSeekAsync("conn2", seeker, pool, CT)).IsError.Should().BeFalse();

        await _casualPoolGrainMock.Received(2).AddSeekAsync(seeker, grain, CT);
        _stateStats.Writes.Should().BeGreaterThanOrEqualTo(1);
        _stateStats.Clears.Should().Be(0);
        _state
            .ConnectionMap.PoolConnections(pool)
            .Should()
            .BeEquivalentTo([(ConnectionId)"conn1", (ConnectionId)"conn2"]);
    }

    [Fact]
    public async Task CreateSeekAsync_rejects_when_too_many_active_games()
    {
        var seeker = new CasualSeekerFaker(_userId).Generate();
        var pool = new PoolKeyFaker().Generate();

        var grain = await Silo.CreateGrainAsync<PlayerSessionGrain>(_userId);
        await FillGameLimitAsync(grain, seeker, pool);

        var result = await grain.CreateSeekAsync("extra-conn", seeker, pool, CT);

        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(PlayerSessionErrors.TooManyGames);
        _state.ConnectionMap.PoolConnections(pool).Should().BeEmpty();
    }

    [Fact]
    public async Task CreateSeekAsync_rejects_connection_already_in_game()
    {
        var pool = new PoolKeyFaker().Generate();
        var seeker = new RatedSeekerFaker(_userId).Generate();

        var grain = await Silo.CreateGrainAsync<PlayerSessionGrain>(_userId);
        await grain.CreateSeekAsync("conn1", seeker, pool, CT);
        await grain.SeekMatchedAsync(new OngoingGameFaker(pool).Generate(), CT);

        var result = await grain.CreateSeekAsync("conn1", seeker, pool, CT);

        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(PlayerSessionErrors.ConnectionInGame);
        _state.ConnectionMap.PoolConnections(pool).Should().BeEmpty();
    }

    [Fact]
    public async Task CleanupConnectionAsync_only_cancels_the_seek_when_the_connection_is_the_only_in_pool()
    {
        var poolToRemove = new PoolKeyFaker(PoolType.Casual).Generate();
        var poolStillActive = new PoolKeyFaker(PoolType.Rated).Generate();
        var seeker = new RatedSeekerFaker(_userId).Generate();

        var grain = await Silo.CreateGrainAsync<PlayerSessionGrain>(_userId);
        await grain.CreateSeekAsync("conn1", seeker, poolToRemove, CT);
        await grain.CreateSeekAsync("conn2", seeker, poolStillActive, CT);
        await grain.CreateSeekAsync("conn3", seeker, poolToRemove, CT); // keeps poolToRemove active
        _stateStats.ResetCounts();

        await grain.CleanupConnectionAsync("conn1", CT);

        await _casualPoolGrainMock.DidNotReceiveWithAnyArgs().TryCancelSeekAsync(default!, CT);
        await _ratedPoolGrainMock.DidNotReceiveWithAnyArgs().TryCancelSeekAsync(default!, CT);
        _state
            .ConnectionMap.PoolConnections(poolStillActive)
            .Should()
            .BeEquivalentTo([(ConnectionId)"conn2"]);
        _state
            .ConnectionMap.PoolConnections(poolToRemove)
            .Should()
            .BeEquivalentTo([(ConnectionId)"conn3"]);
        _stateStats.Writes.Should().BeGreaterThanOrEqualTo(1);
        _stateStats.Clears.Should().Be(0);
    }

    [Fact]
    public async Task CleanupConnectionAsync_removes_connection_from_multiple_pools()
    {
        var pool1 = new PoolKeyFaker(PoolType.Casual).Generate();
        var pool2 = new PoolKeyFaker(PoolType.Rated).Generate();
        var seeker = new RatedSeekerFaker(_userId).Generate();

        var grain = await Silo.CreateGrainAsync<PlayerSessionGrain>(_userId);
        await grain.CreateSeekAsync("conn1", seeker, pool1, CT);
        await grain.CreateSeekAsync("conn1", seeker, pool2, CT);
        _stateStats.ResetCounts();

        await grain.CleanupConnectionAsync("conn1", CT);

        await _casualPoolGrainMock.Received(1).TryCancelSeekAsync(_userId, CT);
        await _ratedPoolGrainMock.Received(1).TryCancelSeekAsync(_userId, CT);
        _state.ConnectionMap.ActivePools.Should().BeEmpty();
        _stateStats.Writes.Should().Be(0);
        _stateStats.Clears.Should().Be(1);
    }

    [Fact]
    public async Task CleanupConnectionAsync_removes_connection_from_recently_matched_and_allows_new_seek()
    {
        var pool = new PoolKeyFaker().Generate();
        var seeker = new CasualSeekerFaker(_userId).Generate();

        var grain = await Silo.CreateGrainAsync<PlayerSessionGrain>(_userId);

        await grain.CreateSeekAsync("conn1", seeker, pool, CT);
        await grain.SeekMatchedAsync(new OngoingGameFaker(pool).Generate(), CT);

        // At this point, conn1 should be in _connectionsRecentlyMatched and blocked
        (await grain.CreateSeekAsync("conn1", seeker, pool, CT))
            .IsError.Should()
            .BeTrue();

        await grain.CleanupConnectionAsync("conn1", CT);
        var result = await grain.CreateSeekAsync("conn1", seeker, pool, CT);
        result.IsError.Should().BeFalse();
    }

    [Fact]
    public async Task CancelSeekAsync_removes_pool_and_notifies_match_failed()
    {
        var pool = new PoolKeyFaker(PoolType.Rated).Generate();
        var seeker = new RatedSeekerFaker(_userId).Generate();

        var grain = await Silo.CreateGrainAsync<PlayerSessionGrain>(_userId);
        await grain.CreateSeekAsync("conn1", seeker, pool, CT);
        await grain.CreateSeekAsync("conn2", seeker, pool, CT);

        await grain.CancelSeekAsync(pool, CT);
        await grain.SeekRemovedAsync(pool, CT);

        List<ConnectionId> expectedConns = ["conn1", "conn2"];
        await _ratedPoolGrainMock.Received(1).TryCancelSeekAsync(_userId, CT);
        await _lobbyNotifier
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
        var ongoingGame = new OngoingGameFaker(poolToMatch).Generate();

        var anoterPool = new PoolKeyFaker(PoolType.Casual).Generate();
        var seeker = new RatedSeekerFaker(_userId).Generate();

        var grain = await Silo.CreateGrainAsync<PlayerSessionGrain>(_userId);
        await grain.CreateSeekAsync("conn1", seeker, poolToMatch, CT);
        await grain.CreateSeekAsync("conn2", seeker, poolToMatch, CT);
        await grain.CreateSeekAsync("conn3", seeker, anoterPool, CT);

        await grain.SeekMatchedAsync(ongoingGame, CT);

        List<ConnectionId> expectedConns = ["conn1", "conn2"];
        await _lobbyNotifier
            .Received(1)
            .NotifyGameFoundAsync(
                _userId,
                Arg.Is<IEnumerable<ConnectionId>>(ids => ids.SequenceEqual(expectedConns)),
                ongoingGame
            );

        // after matching the pool should be removed
        (await grain.TryReserveSeekAsync(poolToMatch))
            .Should()
            .BeFalse();

        _state.ConnectionMap.ActivePools.Should().BeEquivalentTo([anoterPool]);
        _stateStats.Writes.Should().BeGreaterThanOrEqualTo(1);
        _stateStats.Clears.Should().Be(0);
    }

    [Fact]
    public async Task SeekRemovedAsync_notifies_match_failed()
    {
        var pool = new PoolKeyFaker().Generate();
        var seeker = new CasualSeekerFaker(_userId).Generate();

        var grain = await Silo.CreateGrainAsync<PlayerSessionGrain>(_userId);
        await grain.CreateSeekAsync("conn1", seeker, pool, CT);
        await grain.CreateSeekAsync("conn2", seeker, pool, CT);
        _stateStats.ResetCounts();

        await grain.SeekRemovedAsync(pool, CT);

        List<ConnectionId> expectedConns = ["conn1", "conn2"];
        await _lobbyNotifier
            .Received(1)
            .NotifySeekFailedAsync(
                Arg.Is<IEnumerable<ConnectionId>>(ids => ids.SequenceEqual(expectedConns)),
                pool
            );

        _state.ConnectionMap.ActivePools.Should().BeEmpty();
        _stateStats.Writes.Should().Be(0);
        _stateStats.Clears.Should().BeGreaterThanOrEqualTo(1);
    }

    [Fact]
    public async Task TryReserveSeekAsync_returns_false_if_game_limit_reached()
    {
        PoolKey pool = new(PoolType.Rated, new TimeControlSettings(180, 2));
        var seeker = new RatedSeekerFaker(_userId).Generate();

        var grain = await Silo.CreateGrainAsync<PlayerSessionGrain>(_userId);
        await FillGameLimitAsync(grain, seeker, pool);

        (await grain.TryReserveSeekAsync(pool)).Should().BeFalse();
    }

    [Fact]
    public async Task TryReserveSeekAsync_returns_false_if_pool_already_reserved()
    {
        PoolKey pool = new(PoolType.Casual, new TimeControlSettings(60, 0));
        var seeker = new CasualSeekerFaker(_userId).Generate();

        var grain = await Silo.CreateGrainAsync<PlayerSessionGrain>(_userId);
        await grain.CreateSeekAsync("conn1", seeker, pool, CT);

        (await grain.TryReserveSeekAsync(pool)).Should().BeTrue();
        (await grain.TryReserveSeekAsync(pool)).Should().BeFalse();
    }

    [Fact]
    public async Task TryReserveSeekAsync_returns_false_if_all_connections_claimed_by_other_pools()
    {
        PoolKey pool1 = new(PoolType.Casual, new TimeControlSettings(60, 0));
        PoolKey pool2 = new(PoolType.Rated, new TimeControlSettings(300, 5));
        var seeker = new RatedSeekerFaker(_userId).Generate();

        var grain = await Silo.CreateGrainAsync<PlayerSessionGrain>(_userId);
        // same connection shared across both pools
        await grain.CreateSeekAsync("conn1", seeker, pool1, CT);
        await grain.CreateSeekAsync("conn1", seeker, pool2, CT);

        (await grain.TryReserveSeekAsync(pool1)).Should().BeTrue();
        (await grain.TryReserveSeekAsync(pool2)).Should().BeFalse();
    }

    [Fact]
    public async Task TryReserveSeekAsync_returns_true_when_reservable_connection_exists()
    {
        PoolKey pool1 = new(PoolType.Casual, new TimeControlSettings(60, 0));
        PoolKey pool2 = new(PoolType.Rated, new TimeControlSettings(300, 5));
        var seeker = new RatedSeekerFaker(_userId).Generate();

        var grain = await Silo.CreateGrainAsync<PlayerSessionGrain>(_userId);
        await grain.CreateSeekAsync("conn1", seeker, pool1, CT);
        await grain.CreateSeekAsync("conn2", seeker, pool2, CT);

        (await grain.TryReserveSeekAsync(pool1)).Should().BeTrue(); // reserves conn1
        (await grain.TryReserveSeekAsync(pool2)).Should().BeTrue(); // reserves conn2
    }

    [Fact]
    public async Task ReleaseReservationAsync_removes_claim()
    {
        PoolKey pool = new(PoolType.Rated, new TimeControlSettings(180, 2));
        var seeker = new RatedSeekerFaker(_userId).Generate();

        var grain = await Silo.CreateGrainAsync<PlayerSessionGrain>(_userId);
        await grain.CreateSeekAsync("conn1", seeker, pool, CT);

        (await grain.TryReserveSeekAsync(pool)).Should().BeTrue();
        await grain.ReleaseReservationAsync(pool);
        (await grain.TryReserveSeekAsync(pool)).Should().BeTrue();
    }

    [Fact]
    public async Task MatchWithOpenSeekAsync_starts_game_and_notifies()
    {
        var grain = await Silo.CreateGrainAsync<PlayerSessionGrain>(_userId);

        var seeker = new CasualSeekerFaker(_userId).Generate();
        var ongoingGame = new OngoingGameFaker(
            new PoolKey(PoolType.Casual, new TimeControlSettings(60, 0))
        ).Generate();
        ConnectionId connection = "conn1";
        UserId targetSeekerId = "target-user";

        _casualPoolGrainMock.MatchWithSeekerAsync(seeker, targetSeekerId, CT).Returns(ongoingGame);

        var result = await grain.MatchWithOpenSeekAsync(
            connection,
            seeker,
            targetSeekerId,
            ongoingGame.Pool,
            CT
        );

        result.IsError.Should().BeFalse();
        List<ConnectionId> expectedConns = [connection];
        await _casualPoolGrainMock.Received(1).MatchWithSeekerAsync(seeker, targetSeekerId, CT);
        await _lobbyNotifier
            .Received(1)
            .NotifyGameFoundAsync(
                _userId,
                Arg.Is<IEnumerable<ConnectionId>>(ids => ids.SequenceEqual(expectedConns)),
                ongoingGame
            );

        _state
            .OngoingGames.Should()
            .BeEquivalentTo(
                new Dictionary<GameToken, OngoingGame>() { [ongoingGame.GameToken] = ongoingGame }
            );
        _stateStats.Writes.Should().BeGreaterThanOrEqualTo(1);
        _stateStats.Clears.Should().Be(0);
    }

    [Fact]
    public async Task MatchWithOpenSeekAsync_returns_error_if_game_limit_reached()
    {
        var pool = new PoolKey(PoolType.Casual, new TimeControlSettings(60, 0));
        var grain = await Silo.CreateGrainAsync<PlayerSessionGrain>(_userId);

        var seeker = new CasualSeekerFaker(_userId).Generate();
        await FillGameLimitAsync(grain, seeker, pool);

        var result = await grain.MatchWithOpenSeekAsync("connX", seeker, "any-user", pool, CT);

        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(PlayerSessionErrors.TooManyGames);
        await _casualPoolGrainMock
            .DidNotReceiveWithAnyArgs()
            .MatchWithSeekerAsync(default!, default!, CT);
    }

    [Fact]
    public async Task MatchWithOpenSeekAsync_returns_error_if_connection_taken()
    {
        var pool = new PoolKey(PoolType.Casual, new TimeControlSettings(60, 0));
        var grain = await Silo.CreateGrainAsync<PlayerSessionGrain>(_userId);

        var seeker = new CasualSeekerFaker(_userId).Generate();
        await grain.CreateSeekAsync("conn1", seeker, pool, CT);
        await grain.SeekMatchedAsync(new OngoingGameFaker(pool).Generate(), CT); // now conn1 is in a game

        var result = await grain.MatchWithOpenSeekAsync("conn1", seeker, "any-user", pool, CT);

        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(PlayerSessionErrors.ConnectionInGame);
        await _casualPoolGrainMock
            .DidNotReceiveWithAnyArgs()
            .MatchWithSeekerAsync(default!, default!, CT);
    }

    [Fact]
    public async Task MatchWithOpenSeekAsync_returns_error_when_matchmaking_grain_returns_error()
    {
        var pool = new PoolKey(PoolType.Casual, new TimeControlSettings(60, 0));
        var grain = await Silo.CreateGrainAsync<PlayerSessionGrain>(_userId);

        var seeker = new CasualSeekerFaker(_userId).Generate();
        var targetSeekerId = "target-user";

        _casualPoolGrainMock
            .MatchWithSeekerAsync(seeker, targetSeekerId, CT)
            .Returns(MatchmakingErrors.RequestedSeekerNotCompatible);

        var result = await grain.MatchWithOpenSeekAsync("conn1", seeker, targetSeekerId, pool, CT);

        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(MatchmakingErrors.RequestedSeekerNotCompatible);
        await _lobbyNotifier
            .DidNotReceiveWithAnyArgs()
            .NotifyGameFoundAsync(default, default!, default!);
    }

    [Fact]
    public async Task GameEndedEvent_removes_games_from_active_games()
    {
        var ongoingGame = new OngoingGameFaker().Generate();
        var seeker = new RatedSeekerFaker(_userId).Generate();

        var streamProbe = ProbeGameEndedStream();
        var grain = await Silo.CreateGrainAsync<PlayerSessionGrain>(_userId);

        await grain.CreateSeekAsync("conn1", seeker, ongoingGame.Pool, CT);
        await grain.SeekMatchedAsync(ongoingGame, CT);

        _state
            .OngoingGames.Should()
            .BeEquivalentTo(
                new Dictionary<GameToken, OngoingGame>() { [ongoingGame.GameToken] = ongoingGame }
            );

        await streamProbe.OnNextAsync(
            new GameEndedEvent(ongoingGame.GameToken, new GameResultDataFaker().Generate())
        );

        await _lobbyNotifier
            .Received(1)
            .NotifyOngoingGameEndedAsync(_userId, ongoingGame.GameToken);
        _state.OngoingGames.Should().BeEmpty();

        _stateStats.Writes.Should().BeGreaterThanOrEqualTo(1);
    }

    [Fact]
    public async Task GetOngoingGamesAsync_returns_all_ongoing_games()
    {
        var grain = await Silo.CreateGrainAsync<PlayerSessionGrain>(_userId);

        var seeker = new RatedSeekerFaker(_userId).Generate();
        PoolKey[] pools =
        [
            new(PoolType.Rated, new(1, 2)),
            new(PoolType.Casual, new(3, 4)),
            new(PoolType.Rated, new(4, 5)),
        ];
        List<OngoingGame> games = [];
        for (int i = 0; i < pools.Length; i++)
        {
            var pool = pools[i];
            await grain.CreateSeekAsync($"conn{i}", seeker, pool, CT);

            var game = new OngoingGameFaker(pool).Generate();
            await grain.SeekMatchedAsync(game, CT);
            games.Add(game);
        }

        var result = await grain.GetOngoingGamesAsync();

        result.Should().BeEquivalentTo(games);
    }

    private async Task FillGameLimitAsync(PlayerSessionGrain grain, Seeker seeker, PoolKey pool)
    {
        for (var i = 0; i < _settings.MaxActiveGames; i++)
        {
            await grain.CreateSeekAsync($"conn{i}", seeker, pool);
            await grain.SeekMatchedAsync(new OngoingGameFaker(pool).Generate(), CT);
        }
    }
}
