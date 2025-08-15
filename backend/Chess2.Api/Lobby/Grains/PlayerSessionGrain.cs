using Chess2.Api.Infrastructure;
using Chess2.Api.Infrastructure.Extensions;
using Chess2.Api.Lobby.Errors;
using Chess2.Api.Lobby.Services;
using Chess2.Api.Matchmaking.Models;
using Chess2.Api.Matchmaking.Stream;
using Chess2.Api.Shared.Models;
using Chess2.Api.Users.Models;
using ErrorOr;
using Microsoft.Extensions.Options;
using Orleans.Streams;

namespace Chess2.Api.Lobby.Grains;

[Alias("Chess2.Api.Lobby.Grains.IPlayerSessionGrain")]
public interface IPlayerSessionGrain : IGrainWithStringKey
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
    private readonly Dictionary<
        PoolKey,
        StreamSubscriptionHandle<PlayerSeekEndedEvent>
    > _seekSubs = [];

    private readonly HashSet<string> _activeGameTokens = [];

    private readonly ILogger<PlayerSessionGrain> _logger;
    private readonly ILobbyNotifier _matchmakingNotifier;
    private readonly LobbySettings _settings;

    private IStreamProvider _streamProvider = null!;

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

        var matchmakingGrain = GrainFactory.GetMatchmakingGrain(pool);
        await matchmakingGrain.AddSeekAsync(seeker);

        _connectionMap.AddConnectionToPool(connectionId, pool);

        if (!_seekSubs.ContainsKey(pool))
        {
            var seekStream = _streamProvider.GetStream<PlayerSeekEndedEvent>(
                MatchmakingStreamConstants.PlayerSeekEndedStream,
                MatchmakingStreamKey.SeekStream(_userId, pool)
            );
            _seekSubs[pool] = await seekStream.SubscribeAsync(
                (@event, token) => SeekEndedAsync(@event.GameToken, pool)
            );
        }

        return Result.Created;
    }

    public async Task CleanupConnectionAsync(ConnectionId connectionId)
    {
        var removedPools = _connectionMap.RemoveConnection(connectionId);
        foreach (var pool in removedPools)
        {
            await UnsubscribeFromSeekAsync(pool);
            await GrainFactory.GetMatchmakingGrain(pool).TryCancelSeekAsync(_userId);
        }
    }

    public async Task CancelSeekAsync(PoolKey pool)
    {
        await UnsubscribeFromSeekAsync(pool);
        await GrainFactory.GetMatchmakingGrain(pool).TryCancelSeekAsync(_userId);

        var poolConnectionIds = _connectionMap.RemovePool(pool);
        await _matchmakingNotifier.NotifyMatchFailedAsync(poolConnectionIds);
    }

    public Task GameEndedAsync(string gameToken) =>
        Task.FromResult(_activeGameTokens.Remove(gameToken));

    public override Task OnActivateAsync(CancellationToken cancellationToken)
    {
        _streamProvider = this.GetStreamProvider(Streaming.StreamProvider);
        return base.OnActivateAsync(cancellationToken);
    }

    private async Task SeekEndedAsync(string? gameToken, PoolKey pool)
    {
        var poolConnectionIds = _connectionMap.RemovePool(pool);
        if (gameToken is not null)
        {
            await _matchmakingNotifier.NotifyGameFoundAsync(poolConnectionIds, gameToken);
            _activeGameTokens.Add(gameToken);
        }
        else
        {
            await _matchmakingNotifier.NotifyMatchFailedAsync(poolConnectionIds);
        }

        foreach (var connectionId in poolConnectionIds)
        {
            await CleanupConnectionAsync(connectionId);
        }
        await UnsubscribeFromSeekAsync(pool);
    }

    private async Task UnsubscribeFromSeekAsync(PoolKey pool)
    {
        if (_seekSubs.TryGetValue(pool, out var subscription))
        {
            await subscription.UnsubscribeAsync();
            _seekSubs.Remove(pool);
        }
    }

    private bool HasReachedGameLimit() =>
        _connectionMap.SeekCount + _activeGameTokens.Count >= _settings.MaxActiveGames;
}
