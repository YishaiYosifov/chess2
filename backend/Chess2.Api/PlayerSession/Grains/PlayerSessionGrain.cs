using Chess2.Api.Infrastructure;
using Chess2.Api.Matchmaking.Grains;
using Chess2.Api.Matchmaking.Models;
using Chess2.Api.Matchmaking.Services;
using Chess2.Api.Matchmaking.Stream;
using Chess2.Api.Shared.Models;
using Chess2.Api.Users.Models;
using Orleans.Streams;

namespace Chess2.Api.PlayerSession.Grains;

[Alias("Chess2.Api.PlayerSession.Grains.IPlayerSessionGrain")]
public interface IPlayerSessionGrain : IGrainWithStringKey
{
    [Alias("CreateSeekAsync")]
    Task CreateSeekAsync(ConnectionId connectionId, Seeker seeker, PoolKey pool);

    [Alias("CancelSeekAsync")]
    Task CancelSeekAsync(ConnectionId connectionId);

    [Alias("GameEndedAsync")]
    Task GameEndedAsync(string gameToken);
}

public class SeekNotificationSession
{
    public required HashSet<ConnectionId> TargetConnections { get; init; }
    public required StreamSubscriptionHandle<SeekMatchedEvent> SeekSubscription { get; init; }
}

public class PlayerSessionGrain : Grain, IPlayerSessionGrain, IGrainBase
{
    private readonly UserId _userId;
    private readonly Dictionary<ConnectionId, PoolKey> _connectionToPool = [];
    private readonly Dictionary<PoolKey, SeekNotificationSession> _seekSessions = [];
    private readonly HashSet<string> _activeGameTokens = [];

    private readonly ILogger<PlayerSessionGrain> _logger;
    private readonly IGrainFactory _grains;
    private readonly IMatchmakingNotifier _matchmakingNotifier;

    private IStreamProvider _streamProvider = null!;

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

    public async Task CreateSeekAsync(ConnectionId connectionId, Seeker seeker, PoolKey pool)
    {
        DelayDeactivation(TimeSpan.FromMinutes(5));

        await CancelSeekIfExistsAsync(connectionId);
        if (_seekSessions.TryGetValue(pool, out var existingSeekSession))
        {
            _connectionToPool.TryAdd(connectionId, pool);
            existingSeekSession.TargetConnections.Add(connectionId);
            return;
        }

        var matchmakingGrain = ResolvePoolGrain(pool);
        var isSeekSuccess = await matchmakingGrain.TryCreateSeekAsync(seeker);
        if (!isSeekSuccess)
            return;

        var stream = _streamProvider.GetStream<SeekMatchedEvent>(
            MatchmakingStreamConstants.SeekMatchedStream,
            MatchmakingStreamKey.MatchedStream(_userId, pool)
        );
        var subscription = await stream.SubscribeAsync(
            (@event, token) => MatchFoundAsync(@event.GameToken, pool)
        );
        _connectionToPool.TryAdd(connectionId, pool);

        SeekNotificationSession seekSession = new()
        {
            TargetConnections = [connectionId],
            SeekSubscription = subscription,
        };
        _seekSessions.TryAdd(pool, seekSession);
    }

    public Task CancelSeekAsync(ConnectionId connectionId) => CancelSeekIfExistsAsync(connectionId);

    public Task GameEndedAsync(string gameToken) =>
        Task.FromResult(_activeGameTokens.Remove(gameToken));

    private async Task MatchFoundAsync(string gameToken, PoolKey pool)
    {
        _activeGameTokens.Add(gameToken);
        await Task.CompletedTask;

        if (!_seekSessions.TryGetValue(pool, out var seekSession))
            return;

        await Task.WhenAll(
            seekSession.TargetConnections.Select(connId =>
                _matchmakingNotifier.NotifyGameFoundAsync(connId, gameToken)
            )
        );
        await RemovePoolMappingAsync(pool);
    }

    public override Task OnActivateAsync(CancellationToken cancellationToken)
    {
        _streamProvider = this.GetStreamProvider(Streaming.StreamProvider);
        return base.OnActivateAsync(cancellationToken);
    }

    public override async Task OnDeactivateAsync(
        DeactivationReason reason,
        CancellationToken cancellationToken
    )
    {
        await Task.WhenAll(
            _seekSessions.Values.Select(session => session.SeekSubscription.UnsubscribeAsync())
        );
        await base.OnDeactivateAsync(reason, cancellationToken);
    }

    private async Task CancelSeekIfExistsAsync(ConnectionId connectionId)
    {
        if (!_connectionToPool.TryGetValue(connectionId, out var pool))
            return;

        var poolGrain = ResolvePoolGrain(pool);
        await poolGrain.TryCancelSeekAsync(_userId);

        if (_seekSessions.TryGetValue(pool, out var seekSession))
        {
            await Task.WhenAll(
                seekSession.TargetConnections.Select(notifyConnId =>
                {
                    if (notifyConnId != connectionId)
                        return _matchmakingNotifier.NotifyMatchFailedAsync(notifyConnId);
                    return Task.CompletedTask;
                })
            );
        }

        await RemovePoolMappingAsync(pool);
    }

    private async Task RemovePoolMappingAsync(PoolKey pool)
    {
        if (!_seekSessions.TryGetValue(pool, out var seekSession))
            return;

        foreach (var connectionId in seekSession.TargetConnections)
        {
            _connectionToPool.Remove(connectionId);
        }
        await seekSession.SeekSubscription.UnsubscribeAsync();
        _seekSessions.Remove(pool);
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
