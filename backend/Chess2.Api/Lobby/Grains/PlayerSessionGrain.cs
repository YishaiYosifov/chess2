using Chess2.Api.Infrastructure.Extensions;
using Chess2.Api.Lobby.Errors;
using Chess2.Api.Lobby.Services;
using Chess2.Api.Matchmaking.Grains;
using Chess2.Api.Matchmaking.Models;
using Chess2.Api.Shared.Models;
using Chess2.Api.Users.Models;
using ErrorOr;
using Microsoft.Extensions.Options;

namespace Chess2.Api.Lobby.Grains;

[Alias("Chess2.Api.Lobby.Grains.IPlayerSessionGrain")]
public interface IPlayerSessionGrain : IGrainWithStringKey, ISeekObserver
{
    [Alias("CreateSeekAsync")]
    Task<ErrorOr<Created>> CreateSeekAsync(ConnectionId connectionId, Seeker seeker, PoolKey pool);

    [Alias("CleanupConnectionAsync")]
    Task CleanupConnectionAsync(ConnectionId connectionId);

    [Alias("CancelSeekAsync")]
    Task CancelSeekAsync(PoolKey pool);

    [Alias("GameEndedAsync")]
    Task GameEndedAsync(string gameToken);
}

public class PlayerSessionGrain : Grain, IPlayerSessionGrain, IGrainBase
{
    private readonly UserId _userId;

    private readonly PlayerConnectionPoolMap _connectionMap = new();
    private readonly Dictionary<string, HashSet<ConnectionId>> _activeGameTokens = [];
    private readonly Dictionary<PoolKey, ConnectionId> _poolClaims = [];

    private readonly ILogger<PlayerSessionGrain> _logger;
    private readonly ILobbyNotifier _matchmakingNotifier;
    private readonly LobbySettings _settings;

    public PlayerSessionGrain(
        ILogger<PlayerSessionGrain> logger,
        ILobbyNotifier matchmakingNotifier,
        IOptions<AppSettings> settings
    )
    {
        _userId = this.GetPrimaryKeyString();

        _logger = logger;
        _matchmakingNotifier = matchmakingNotifier;
        _settings = settings.Value.Lobby;
    }

    public async Task<ErrorOr<Created>> CreateSeekAsync(
        ConnectionId connectionId,
        Seeker seeker,
        PoolKey pool
    )
    {
        if (HasReachedGameLimit())
            return PlayerSessionErrors.TooManyGames;

        if (IsConnectionTaken(connectionId))
            return PlayerSessionErrors.ConnectionInGame;

        var matchmakingGrain = GrainFactory.GetMatchmakingGrain(pool);
        await matchmakingGrain.AddSeekAsync(seeker, this.AsSafeReference<ISeekObserver>());

        _connectionMap.AddConnectionToPool(connectionId, pool);

        return Result.Created;
    }

    public async Task CleanupConnectionAsync(ConnectionId connectionId)
    {
        var removedPools = _connectionMap.RemoveConnection(connectionId);
        foreach (var pool in removedPools)
        {
            await GrainFactory.GetMatchmakingGrain(pool).TryCancelSeekAsync(_userId);
        }
    }

    public Task CancelSeekAsync(PoolKey pool) =>
        GrainFactory.GetMatchmakingGrain(pool).TryCancelSeekAsync(_userId);

    public Task GameEndedAsync(string gameToken) =>
        Task.FromResult(_activeGameTokens.Remove(gameToken));

    public async Task SeekMatchedAsync(string gameToken, PoolKey pool)
    {
        _poolClaims.Remove(pool);

        var poolConnectionIds = _connectionMap.RemovePool(pool);
        await _matchmakingNotifier.NotifyGameFoundAsync(poolConnectionIds, gameToken);
        _activeGameTokens.Add(gameToken, poolConnectionIds);

        foreach (var connectionId in poolConnectionIds)
        {
            await CleanupConnectionAsync(connectionId);
        }

        if (HasReachedGameLimit())
            await CancelAllSeeksAsync();
    }

    public async Task SeekRemovedAsync(PoolKey pool)
    {
        _poolClaims.Remove(pool);

        var poolConnectionIds = _connectionMap.RemovePool(pool);
        await _matchmakingNotifier.NotifySeekFailedAsync(poolConnectionIds, pool);
    }

    public Task<bool> TryReserveSeekAsync(PoolKey pool)
    {
        if (HasReachedGameLimit())
            return Task.FromResult(false);
        if (_poolClaims.ContainsKey(pool))
            return Task.FromResult(false);

        var connectionIds = _connectionMap.PoolConnections(pool);

        foreach (var connectionId in connectionIds)
        {
            if (!IsConnectionTaken(connectionId))
            {
                _poolClaims[pool] = connectionId;
                return Task.FromResult(true);
            }
        }

        return Task.FromResult(false);
    }

    public Task ReleaseReservationAsync(PoolKey pool)
    {
        _poolClaims.Remove(pool);
        return Task.CompletedTask;
    }

    private async Task CancelAllSeeksAsync()
    {
        foreach (var pool in _connectionMap.ActivePools)
        {
            await GrainFactory.GetMatchmakingGrain(pool).TryCancelSeekAsync(_userId);
        }
        _connectionMap.RemoveAllPools();
    }

    private bool IsConnectionTaken(ConnectionId connectionId) =>
        _activeGameTokens.Values.Any(tokens => tokens.Contains(connectionId))
        || _poolClaims.Values.Any(claimedConn => connectionId == claimedConn);

    private bool HasReachedGameLimit() =>
        _activeGameTokens.Count + _poolClaims.Count >= _settings.MaxActiveGames;
}
