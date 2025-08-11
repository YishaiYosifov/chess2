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
    public required StreamSubscriptionHandle<SeekEndedEvent> SeekEndedSubscription { get; init; }
}

public class PlayerSessionGrain : Grain, IPlayerSessionGrain, IGrainBase
{
    private readonly UserId _userId;
    private readonly Dictionary<ConnectionId, PoolKey> _connectionToPool = [];
    private readonly Dictionary<PoolKey, SeekNotificationSession> _seekSessions = [];
    private readonly HashSet<string> _activeGameTokens = [];

    private readonly ILogger<PlayerSessionGrain> _logger;
    private readonly IGrainFactory _grains;
    private readonly ILobbyNotifier _matchmakingNotifier;

    private IStreamProvider _streamProvider = null!;

    public PlayerSessionGrain(
        ILogger<PlayerSessionGrain> logger,
        IGrainFactory grains,
        ILobbyNotifier matchmakingNotifier
    )
    {
        _userId = this.GetPrimaryKeyString();

        _logger = logger;
        _grains = grains;
        _matchmakingNotifier = matchmakingNotifier;
    }

    public async Task CreateSeekAsync(ConnectionId connectionId, Seeker seeker, PoolKey pool)
    {
        await CancelSeekIfExistsAsync(connectionId);

        var matchmakingGrain = ResolvePoolGrain(pool);
        await matchmakingGrain.AddSeekAsync(seeker);

        _connectionToPool.TryAdd(connectionId, pool);
        if (_seekSessions.TryGetValue(pool, out var existingSeekSession))
        {
            existingSeekSession.TargetConnections.Add(connectionId);
            return;
        }

        var seekStream = _streamProvider.GetStream<SeekEndedEvent>(
            MatchmakingStreamConstants.EndedStream,
            MatchmakingStreamKey.SeekStream(_userId, pool)
        );
        SeekNotificationSession seekSession = new()
        {
            TargetConnections = [connectionId],
            SeekEndedSubscription = await seekStream.SubscribeAsync(
                (@event, token) => SeekEndedAsync(@event.GameToken, pool)
            ),
        };
        _seekSessions.TryAdd(pool, seekSession);
    }

    public Task CancelSeekAsync(ConnectionId connectionId) => CancelSeekIfExistsAsync(connectionId);

    public Task GameEndedAsync(string gameToken) =>
        Task.FromResult(_activeGameTokens.Remove(gameToken));

    public override Task OnActivateAsync(CancellationToken cancellationToken)
    {
        _streamProvider = this.GetStreamProvider(Streaming.StreamProvider);
        return base.OnActivateAsync(cancellationToken);
    }

    private async Task SeekEndedAsync(string? gameToken, PoolKey pool)
    {
        if (!_seekSessions.TryGetValue(pool, out var seekSession))
            return;

        if (gameToken is not null)
        {
            await Task.WhenAll(
                seekSession.TargetConnections.Select(connId =>
                    _matchmakingNotifier.NotifyGameFoundAsync(connId, gameToken)
                )
            );
            _activeGameTokens.Add(gameToken);
        }
        else
        {
            await Task.WhenAll(
                seekSession.TargetConnections.Select(notifyConnId =>
                    _matchmakingNotifier.NotifyMatchFailedAsync(notifyConnId)
                )
            );
        }

        foreach (var connectionId in seekSession.TargetConnections)
        {
            _connectionToPool.Remove(connectionId);
        }
        await seekSession.SeekEndedSubscription.UnsubscribeAsync();
        _seekSessions.Remove(pool);
    }

    private async Task CancelSeekIfExistsAsync(ConnectionId connectionId)
    {
        if (!_connectionToPool.TryGetValue(connectionId, out var pool))
            return;

        var poolGrain = ResolvePoolGrain(pool);
        await poolGrain.TryCancelSeekAsync(_userId);
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
