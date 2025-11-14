using Chess2.Api.Game.Models;
using Chess2.Api.Infrastructure;
using Chess2.Api.Infrastructure.Extensions;
using Chess2.Api.Lobby.Errors;
using Chess2.Api.Lobby.Services;
using Chess2.Api.Matchmaking.Extensions;
using Chess2.Api.Matchmaking.Grains;
using Chess2.Api.Matchmaking.Models;
using Chess2.Api.Profile.Models;
using Chess2.Api.Shared.Models;
using ErrorOr;
using Microsoft.Extensions.Options;
using Orleans.Streams;

namespace Chess2.Api.Lobby.Grains;

[Alias("Chess2.Api.Lobby.Grains.IPlayerSessionGrain")]
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
}

[GenerateSerializer]
[Alias("Chess2.Api.Lobby.Grains.PlayerSessionState")]
public class PlayerSessionState
{
    [Id(0)]
    public PlayerConnectionPoolMap ConnectionMap { get; } = new();

    [Id(1)]
    public HashSet<string> ActiveGameTokens { get; } = [];
}

[ImplicitStreamSubscription(nameof(GameEndedEvent))]
public class PlayerSessionGrain : Grain, IPlayerSessionGrain, IAsyncObserver<GameEndedEvent>
{
    public const string StateName = "playerSession";

    private readonly UserId _userId;

    private readonly Dictionary<PoolKey, ConnectionId> _poolConnectionReservations = [];
    private readonly HashSet<ConnectionId> _connectionsRecentlyMatched = [];

    private readonly IPersistentState<PlayerSessionState> _state;
    private readonly ILogger<PlayerSessionGrain> _logger;
    private readonly ILobbyNotifier _matchmakingNotifier;
    private readonly LobbySettings _settings;

    public PlayerSessionGrain(
        [PersistentState(StateName, Storage.StorageProvider)]
            IPersistentState<PlayerSessionState> state,
        ILogger<PlayerSessionGrain> logger,
        ILobbyNotifier matchmakingNotifier,
        IOptions<AppSettings> settings
    )
    {
        _userId = this.GetPrimaryKeyString();

        _state = state;
        _logger = logger;
        _matchmakingNotifier = matchmakingNotifier;
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
        await _state.WriteStateAsync(token);

        return Result.Created;
    }

    public async Task CleanupConnectionAsync(
        ConnectionId connectionId,
        CancellationToken token = default
    )
    {
        await RemoveConnectionFromPoolsAsync(connectionId, token);
        _connectionsRecentlyMatched.Remove(connectionId);
        await _state.WriteStateAsync(token);
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

        var startGameResult = await GrainFactory
            .GetMatchmakingGrain(pool)
            .MatchWithSeekerAsync(seeker, matchWith, token);
        if (startGameResult.IsError)
            return startGameResult.Errors;
        var gameToken = startGameResult.Value;

        await OnGameFoundAsync(gameToken, [connectionId], pool, token);
        await _state.WriteStateAsync(token);
        return Result.Created;
    }

    public async Task SeekMatchedAsync(
        GameToken gameToken,
        PoolKey pool,
        CancellationToken token = default
    )
    {
        var poolConnectionIds = _state.State.ConnectionMap.RemovePool(pool);
        await OnGameFoundAsync(gameToken, poolConnectionIds, pool, token);
        await _state.WriteStateAsync(token);
    }

    public async Task SeekRemovedAsync(PoolKey pool, CancellationToken token = default)
    {
        _poolConnectionReservations.Remove(pool);

        var poolConnectionIds = _state.State.ConnectionMap.RemovePool(pool);
        await _state.WriteStateAsync(token);
        await _matchmakingNotifier.NotifySeekFailedAsync(poolConnectionIds, pool);
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

    public override async Task OnActivateAsync(CancellationToken cancellationToken)
    {
        var streamProvider = this.GetStreamProvider(Streaming.StreamProvider);
        var stream = streamProvider.GetStream<GameEndedEvent>(
            nameof(GameEndedEvent),
            this.GetPrimaryKeyString()
        );
        await stream.SubscribeAsync(this);

        await base.OnActivateAsync(cancellationToken);
    }

    public async Task OnNextAsync(GameEndedEvent @event, StreamSequenceToken? token = null)
    {
        _state.State.ActiveGameTokens.Remove(@event.GameToken);
        await _state.WriteStateAsync();
    }

    public Task OnErrorAsync(Exception ex)
    {
        _logger.LogError(ex, "Error in player session grain game stream");
        return Task.CompletedTask;
    }

    private async Task OnGameFoundAsync(
        GameToken gameToken,
        IEnumerable<ConnectionId> connectionIds,
        PoolKey pool,
        CancellationToken token = default
    )
    {
        _state.State.ActiveGameTokens.Add(gameToken);
        _poolConnectionReservations.Remove(pool);

        await _matchmakingNotifier.NotifyGameFoundAsync(connectionIds, gameToken);
        _connectionsRecentlyMatched.UnionWith(connectionIds);

        foreach (var connectionId in connectionIds)
            await RemoveConnectionFromPoolsAsync(connectionId, token);

        if (HasReachedGameLimit())
            await CancelAllSeeksAsync(token);
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
        _state.State.ActiveGameTokens.Count + _poolConnectionReservations.Count
        >= _settings.MaxActiveGames;
}
