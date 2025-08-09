using Chess2.Api.Matchmaking.Grains;
using Chess2.Api.Matchmaking.Models;
using Chess2.Api.Matchmaking.Services;
using Chess2.Api.Shared.Models;
using Chess2.Api.Users.Models;

namespace Chess2.Api.PlayerSession.Grains;

[Alias("Chess2.Api.PlayerSession.Grains.IPlayerSessionGrain")]
public interface IPlayerSessionGrain : IGrainWithStringKey, IMatchObserver
{
    [Alias("CreateSeekAsync")]
    Task CreateSeekAsync(ConnectionId connectionId, Seeker seeker, PoolKey poolKey);

    [Alias("CancelSeekAsync")]
    Task CancelSeekAsync(ConnectionId connectionId);

    [Alias("GameEndedAsync")]
    Task GameEndedAsync(string gameToken);
}

public class PlayerSessionGrain : Grain, IPlayerSessionGrain, IGrainBase
{
    private readonly UserId _userId;
    private readonly Dictionary<ConnectionId, PoolKey> _connectionToPool = [];
    private readonly Dictionary<PoolKey, HashSet<ConnectionId>> _poolToConnections = [];
    private readonly HashSet<string> _activeGameTokens = [];
    private readonly ILogger<PlayerSessionGrain> _logger;
    private readonly IGrainFactory _grains;
    private readonly IMatchmakingNotifier _matchmakingNotifier;

    public PlayerSessionGrain(
        ILogger<PlayerSessionGrain> logger,
        IGrainFactory grains,
        IMatchmakingNotifier matchmakingNotifier
    )
    {
        _userId = this.GetPrimaryKeyString();

        _logger = logger;
        _grains = grains;
        _matchmakingNotifier = matchmakingNotifier;
    }

    public async Task CreateSeekAsync(ConnectionId connectionId, Seeker seeker, PoolKey poolKey)
    {
        await CancelSeekIfExists(connectionId);

        var matchmakingGrain = ResolvePoolGrain(poolKey);
        var isSeekSuccess = await matchmakingGrain.TryCreateSeekAsync(seeker, this);
        if (!isSeekSuccess)
            return;

        _connectionToPool.Add(connectionId, poolKey);

        var connections = _poolToConnections.GetValueOrDefault(poolKey, []);
        connections.Add(connectionId);
        _poolToConnections[poolKey] = connections;
    }

    public Task CancelSeekAsync(ConnectionId connectionId) => CancelSeekIfExists(connectionId);

    public async Task MatchFoundAsync(string gameToken, PoolKey poolKey)
    {
        _activeGameTokens.Add(gameToken);

        var connectionIds = _poolToConnections.GetValueOrDefault(poolKey, []);
        await Task.WhenAll(
            connectionIds.Select(connId =>
                _matchmakingNotifier.NotifyGameFoundAsync(connId, gameToken)
            )
        );
        RemovePoolMapping(poolKey);
    }

    public Task GameEndedAsync(string gameToken) =>
        Task.FromResult(_activeGameTokens.Remove(gameToken));

    private Task KeepSessionAliveAsync()
    {
        if (
            _activeGameTokens.Count != 0
            || _connectionToPool.Count != 0
            || _poolToConnections.Count != 0
        )
            DelayDeactivation(TimeSpan.FromMinutes(2));
        return Task.CompletedTask;
    }

    public override Task OnActivateAsync(CancellationToken cancellationToken)
    {
        this.RegisterGrainTimer(
            callback: KeepSessionAliveAsync,
            dueTime: TimeSpan.Zero,
            period: TimeSpan.FromMinutes(1)
        );

        return base.OnActivateAsync(cancellationToken);
    }

    private async Task CancelSeekIfExists(ConnectionId connectionId)
    {
        if (!_connectionToPool.TryGetValue(connectionId, out var poolKey))
            return;

        var poolGrain = ResolvePoolGrain(poolKey);
        await poolGrain.TryCancelSeekAsync(_userId);

        RemoveConnectionMapping(connectionId);
    }

    private void RemoveConnectionMapping(ConnectionId connectionId)
    {
        if (!_connectionToPool.TryGetValue(connectionId, out var poolKey))
            return;

        _connectionToPool.Remove(connectionId);
        var poolConnections = _poolToConnections.GetValueOrDefault(poolKey, []);
        poolConnections.Remove(connectionId);
        if (poolConnections.Count == 0)
            _poolToConnections.Remove(poolKey);
        else
            _poolToConnections[poolKey] = poolConnections;
    }

    private void RemovePoolMapping(PoolKey poolKey)
    {
        var connectionIds = _poolToConnections.GetValueOrDefault(poolKey, []);

        foreach (var connectionId in connectionIds)
        {
            _connectionToPool.Remove(connectionId);
        }
        _poolToConnections.Remove(poolKey);
    }

    private IMatchmakingGrain ResolvePoolGrain(PoolKey poolKey)
    {
        return poolKey.PoolType switch
        {
            PoolType.Rated => _grains.GetGrain<IRatedMatchmakingGrain>(poolKey.ToGrainKey()),
            PoolType.Casual => _grains.GetGrain<ICasualMatchmakingGrain>(poolKey.ToGrainKey()),
            _ => throw new InvalidOperationException($"Unsupported pool type: {poolKey.PoolType}"),
        };
    }
}
