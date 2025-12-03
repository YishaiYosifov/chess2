using AnarchyChess.Api.Game.Models;
using AnarchyChess.Api.Infrastructure;
using AnarchyChess.Api.Infrastructure.Extensions;
using AnarchyChess.Api.Lobby.Errors;
using AnarchyChess.Api.Lobby.Services;
using AnarchyChess.Api.Matchmaking.Extensions;
using AnarchyChess.Api.Matchmaking.Grains;
using AnarchyChess.Api.Matchmaking.Models;
using AnarchyChess.Api.Profile.Models;
using AnarchyChess.Api.Shared.Models;
using ErrorOr;
using Microsoft.Extensions.Options;
using Orleans.Streams;

namespace AnarchyChess.Api.Lobby.Grains;

[Alias("AnarchyChess.Api.Lobby.Grains.IPlayerSessionGrain")]
public interface IPlayerSessionGrain : IGrainWithStringKey, ISeekObserver
{
    [Alias("CreateSeekAsync")]
    Task<ErrorOr<Created>> CreateSeekAsync(
        ConnectionId connectionId,
        Seeker seeker,
        PoolKey pool,
        CancellationToken token = default
    );

    [Alias("CleanupConnectionAsync")]
    Task CleanupConnectionAsync(ConnectionId connectionId, CancellationToken token = default);

    [Alias("CancelSeekAsync")]
    Task CancelSeekAsync(PoolKey pool, CancellationToken token = default);

    [Alias("MatchWithOpenSeekAsync")]
    Task<ErrorOr<Created>> MatchWithOpenSeekAsync(
        ConnectionId connectionId,
        Seeker seeker,
        UserId matchWith,
        PoolKey pool,
        CancellationToken token = default
    );

    [Alias("GetOngoingGames")]
    Task<List<OngoingGame>> GetOngoingGamesAsync();
}

[GenerateSerializer]
[Alias("AnarchyChess.Api.Lobby.Grains.PlayerSessionState")]
public class PlayerSessionState
{
    [Id(0)]
    public PlayerConnectionPoolMap ConnectionMap { get; } = new();

    [Id(1)]
    public Dictionary<GameToken, OngoingGame> OngoingGames { get; } = [];
}

[ImplicitStreamSubscription(nameof(GameEndedEvent))]
public class PlayerSessionGrain
    : Grain,
        IPlayerSessionGrain,
        IAsyncObserver<GameStartedEvent>,
        IAsyncObserver<GameEndedEvent>
{
    public const string StateName = "playerSession";

    private readonly UserId _userId;

    private readonly Dictionary<PoolKey, ConnectionId> _poolConnectionReservations = [];
    private readonly HashSet<ConnectionId> _connectionsRecentlyMatched = [];

    private readonly IPersistentState<PlayerSessionState> _state;
    private readonly ILogger<PlayerSessionGrain> _logger;
    private readonly ILobbyNotifier _lobbyNotifier;
    private readonly LobbySettings _settings;

    public PlayerSessionGrain(
        [PersistentState(StateName)] IPersistentState<PlayerSessionState> state,
        ILogger<PlayerSessionGrain> logger,
        ILobbyNotifier lobbyNotifier,
        IOptions<AppSettings> settings
    )
    {
        _userId = this.GetPrimaryKeyString();

        _state = state;
        _logger = logger;
        _lobbyNotifier = lobbyNotifier;
        _settings = settings.Value.Lobby;
    }

    public async Task<ErrorOr<Created>> CreateSeekAsync(
        ConnectionId connectionId,
        Seeker seeker,
        PoolKey pool,
        CancellationToken token = default
    )
    {
        if (HasReachedGameLimit())
            return PlayerSessionErrors.TooManyGames;

        if (IsConnectionTaken(connectionId))
            return PlayerSessionErrors.ConnectionInGame;

        var matchmakingGrain = GrainFactory.GetMatchmakingGrain(pool);
        await matchmakingGrain.AddSeekAsync(seeker, this.AsSafeReference<ISeekObserver>(), token);

        _state.State.ConnectionMap.AddConnectionToPool(connectionId, pool);
        await WriteStateAsync(token);

        return Result.Created;
    }

    public async Task CleanupConnectionAsync(
        ConnectionId connectionId,
        CancellationToken token = default
    )
    {
        await RemoveConnectionFromPoolsAsync(connectionId, token);
        _connectionsRecentlyMatched.Remove(connectionId);
        await WriteStateAsync(token);
    }

    public Task CancelSeekAsync(PoolKey pool, CancellationToken token = default) =>
        GrainFactory.GetMatchmakingGrain(pool).TryCancelSeekAsync(_userId, token);

    public async Task<ErrorOr<Created>> MatchWithOpenSeekAsync(
        ConnectionId connectionId,
        Seeker seeker,
        UserId matchWith,
        PoolKey pool,
        CancellationToken token = default
    )
    {
        if (HasReachedGameLimit())
            return PlayerSessionErrors.TooManyGames;

        if (IsConnectionTaken(connectionId))
            return PlayerSessionErrors.ConnectionInGame;

        _state.State.ConnectionMap.AddConnectionToPool(connectionId, pool);
        var startGameResult = await GrainFactory
            .GetMatchmakingGrain(pool)
            .MatchWithSeekerAsync(seeker, matchWith, token);
        if (startGameResult.IsError)
            return startGameResult.Errors;

        await WriteStateAsync(token);
        return Result.Created;
    }

    public async Task SeekRemovedAsync(PoolKey pool, CancellationToken token = default)
    {
        _poolConnectionReservations.Remove(pool);

        var poolConnectionIds = _state.State.ConnectionMap.RemovePool(pool);
        await WriteStateAsync(token);
        await _lobbyNotifier.NotifySeekFailedAsync(poolConnectionIds, pool);
    }

    public Task<bool> TryReserveSeekAsync(PoolKey pool)
    {
        if (HasReachedGameLimit())
            return Task.FromResult(false);
        if (_poolConnectionReservations.ContainsKey(pool))
            return Task.FromResult(false);

        var connectionIds = _state.State.ConnectionMap.PoolConnections(pool);

        foreach (var connectionId in connectionIds)
        {
            if (!IsConnectionTaken(connectionId))
            {
                _poolConnectionReservations[pool] = connectionId;
                return Task.FromResult(true);
            }
        }

        return Task.FromResult(false);
    }

    public Task ReleaseReservationAsync(PoolKey pool)
    {
        _poolConnectionReservations.Remove(pool);
        return Task.CompletedTask;
    }

    public Task<List<OngoingGame>> GetOngoingGamesAsync() =>
        Task.FromResult<List<OngoingGame>>([.. _state.State.OngoingGames.Values]);

    public override async Task OnActivateAsync(CancellationToken cancellationToken)
    {
        var streamProvider = this.GetStreamProvider(Streaming.StreamProvider);

        var startedStream = streamProvider.GetStream<GameStartedEvent>(
            nameof(GameStartedEvent),
            this.GetPrimaryKeyString()
        );
        await startedStream.SubscribeAsync(this);

        var endedStream = streamProvider.GetStream<GameEndedEvent>(
            nameof(GameEndedEvent),
            this.GetPrimaryKeyString()
        );
        await endedStream.SubscribeAsync(this);

        await base.OnActivateAsync(cancellationToken);
    }

    public async Task OnNextAsync(GameEndedEvent @event, StreamSequenceToken? token = null)
    {
        _state.State.OngoingGames.Remove(@event.GameToken);
        await WriteStateAsync();

        await _lobbyNotifier.NotifyOngoingGameEndedAsync(_userId, @event.GameToken);
    }

    public async Task OnNextAsync(GameStartedEvent @event, StreamSequenceToken? token = null)
    {
        var game = @event.Game;
        var connectionIds = _state.State.ConnectionMap.RemovePool(game.Pool);
        _state.State.OngoingGames.Add(game.GameToken, game);
        _poolConnectionReservations.Remove(game.Pool);

        _connectionsRecentlyMatched.UnionWith(connectionIds);

        if (connectionIds.Count > 0)
            await _lobbyNotifier.NotifyGameFoundAsync(_userId, connectionIds, game);

        foreach (var connectionId in connectionIds)
            await RemoveConnectionFromPoolsAsync(connectionId);

        if (HasReachedGameLimit())
            await CancelAllSeeksAsync();

        await WriteStateAsync();
    }

    public Task OnErrorAsync(Exception ex)
    {
        _logger.LogError(ex, "Error in player session grain game stream");
        return Task.CompletedTask;
    }

    private async Task CancelAllSeeksAsync(CancellationToken token = default)
    {
        foreach (var pool in _state.State.ConnectionMap.ActivePools)
        {
            await GrainFactory.GetMatchmakingGrain(pool).TryCancelSeekAsync(_userId, token);
        }
        _state.State.ConnectionMap.RemoveAllPools();
    }

    private async Task RemoveConnectionFromPoolsAsync(
        ConnectionId connectionId,
        CancellationToken token = default
    )
    {
        var removedPools = _state.State.ConnectionMap.RemoveConnection(connectionId);
        foreach (var pool in removedPools)
        {
            await GrainFactory.GetMatchmakingGrain(pool).TryCancelSeekAsync(_userId, token);
        }
    }

    private bool IsConnectionTaken(ConnectionId connectionId) =>
        _connectionsRecentlyMatched.Contains(connectionId)
        || _poolConnectionReservations.Values.Any(claimedConn => connectionId == claimedConn);

    private bool HasReachedGameLimit() =>
        _state.State.OngoingGames.Count + _poolConnectionReservations.Count
        >= _settings.MaxActiveGames;

    private async Task WriteStateAsync(CancellationToken token = default)
    {
        if (_state.State.ConnectionMap.IsEmpty() && _state.State.OngoingGames.Count == 0)
            await _state.ClearStateAsync(token);
        else
            await _state.WriteStateAsync(token);
    }
}
