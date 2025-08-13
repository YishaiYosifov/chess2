using Chess2.Api.Infrastructure;
using Chess2.Api.LiveGame.Services;
using Chess2.Api.Matchmaking.Models;
using Chess2.Api.Matchmaking.Services.Pools;
using Chess2.Api.Matchmaking.Stream;
using Chess2.Api.Shared.Models;
using Chess2.Api.Users.Models;
using Microsoft.Extensions.Options;
using Orleans.Streams;

namespace Chess2.Api.Matchmaking.Grains;

[Alias("Chess2.Api.Matchmaking.Grains.IMatchmakingGrain")]
public interface IMatchmakingGrain : IGrainWithStringKey
{
    [Alias("AddSeekAsync")]
    Task AddSeekAsync(Seeker seeker);

    [Alias("CancelSeekAsync")]
    Task<bool> TryCancelSeekAsync(UserId userId);

    [Alias("GetMatchingSeekersForAsync")]
    Task<List<Seeker>> GetSeekersAsync();
}

public abstract class AbstractMatchmakingGrain<TPool> : Grain, IMatchmakingGrain
    where TPool : IMatchmakingPool
{
    public const int WaveTimer = 0;
    public const int TimeoutTimer = 1;
    private readonly TimeSpan _seekTimeout = TimeSpan.FromMinutes(5);

    private readonly TPool _pool;
    private readonly PoolKey _key;

    private readonly ILogger<AbstractMatchmakingGrain<TPool>> _logger;
    private readonly LobbySettings _settings;
    private readonly IGameStarter _gameStarter;
    private readonly TimeProvider _timeProvider;

    private IStreamProvider _streamProvider = null!;
    private IAsyncStream<OpenSeekCreatedEvent> _openSeekCreatedStream = null!;
    private IAsyncStream<OpenSeekRemovedEvent> _openSeekRemovedStream = null!;

    private readonly Dictionary<UserId, Seeker> _pendingSeekBroadcast = [];

    public AbstractMatchmakingGrain(
        ILogger<AbstractMatchmakingGrain<TPool>> logger,
        IGameStarter gameStarter,
        IOptions<AppSettings> settings,
        TimeProvider timeProvider,
        TPool pool
    )
    {
        _key = PoolKey.FromGrainKey(this.GetPrimaryKeyString());

        _pool = pool;
        _logger = logger;
        _gameStarter = gameStarter;
        _timeProvider = timeProvider;
        _settings = settings.Value.Lobby;
    }

    public Task AddSeekAsync(Seeker seeker)
    {
        _logger.LogInformation("Received create seek from {UserId}", seeker.UserId);

        _pool.AddSeek(seeker);
        _pendingSeekBroadcast.Add(seeker.UserId, seeker);
        return Task.CompletedTask;
    }

    public async Task<bool> TryCancelSeekAsync(UserId userId)
    {
        _logger.LogInformation("Received cancel seek from {UserId}", userId);
        if (!_pool.RemoveSeek(userId))
        {
            _logger.LogInformation("No seek found for user {UserId}", userId);
            return false;
        }

        await NotifySeekEndedAsync(userId);
        await BroadcastSeekRemovedIfNeeded(userId);
        return true;
    }

    public Task<List<Seeker>> GetSeekersAsync()
    {
        return Task.FromResult(_pool.Seekers.ToList());
    }

    private async Task ExecuteWaveAsync()
    {
        var matches = _pool.CalculateMatches();

        List<UserId> removedUserSeeks = [];
        foreach (var (seeker1, seeker2) in matches)
        {
            _logger.LogInformation("Found match for {User1} with {User2}", seeker1, seeker2);

            var isRated = seeker1 is RatedSeeker && seeker2 is RatedSeeker;
            var gameToken = await _gameStarter.StartGameAsync(
                seeker1.UserId,
                seeker2.UserId,
                _key.TimeControl,
                isRated
            );

            await NotifySeekEndedAsync(seeker1.UserId, gameToken);
            await NotifySeekEndedAsync(seeker2.UserId, gameToken);

            if (!_pendingSeekBroadcast.Remove(seeker1.UserId))
                removedUserSeeks.Add(seeker1.UserId);
            if (!_pendingSeekBroadcast.Remove(seeker2.UserId))
                removedUserSeeks.Add(seeker2.UserId);
        }

        await BatchBroadcastOpenSeekRemovedAsync(removedUserSeeks);

        // after a wave we can be sure it wasn't matched, so we can notify about a new open seek
        await BatchBroadcastOpenSeekCreatedAsync(_pendingSeekBroadcast.Values);
        _pendingSeekBroadcast.Clear();
    }

    private async Task TimeoutSeeksAsync()
    {
        var now = _timeProvider.GetUtcNow();

        var timedOutSeekers = _pool
            .Seekers.Where(seeker => now - seeker.CreatedAt >= _seekTimeout)
            .ToList();

        foreach (var seeker in timedOutSeekers)
        {
            _logger.LogInformation(
                "Seek for user {UserId} on pool {Pool} timed out",
                seeker.UserId,
                _key
            );
            _pool.RemoveSeek(seeker.UserId);
            await NotifySeekEndedAsync(seeker.UserId);
        }

        if (_pool.SeekerCount > 0)
            DelayDeactivation(TimeSpan.FromMinutes(5));
        else
            DeactivateOnIdle();
    }

    private Task NotifySeekEndedAsync(UserId userId, string? gameToken = null) =>
        _streamProvider
            .GetStream<PlayerSeekEndedEvent>(
                MatchmakingStreamConstants.PlayerSeekEndedStream,
                MatchmakingStreamKey.SeekStream(userId, _key)
            )
            .OnNextAsync(new(gameToken));

    private async Task BroadcastSeekRemovedIfNeeded(UserId userId)
    {
        // if the seeker is pending broadcast, it means it hasn't been broadcasted yet
        // so no point broadcasting it ended
        if (!_pendingSeekBroadcast.Remove(userId))
        {
            await _openSeekRemovedStream.OnNextAsync(
                new OpenSeekRemovedEvent(new SeekKey(userId, _key))
            );
        }
    }

    private Task BatchBroadcastOpenSeekRemovedAsync(IEnumerable<UserId> userIds) =>
        _openSeekRemovedStream.OnNextBatchAsync(
            userIds.Select(userId => new OpenSeekRemovedEvent(new SeekKey(userId, _key)))
        );

    private Task BatchBroadcastOpenSeekCreatedAsync(IEnumerable<Seeker> seekers) =>
        _openSeekCreatedStream.OnNextBatchAsync(
            seekers.Select(seeker => new OpenSeekCreatedEvent(
                SeekKey: new SeekKey(seeker.UserId, _key),
                Seeker: seeker
            ))
        );

    public override async Task OnActivateAsync(CancellationToken cancellationToken)
    {
        this.RegisterGrainTimer(
            callback: ExecuteWaveAsync,
            dueTime: TimeSpan.Zero,
            period: _settings.MatchWaveEvery
        );
        this.RegisterGrainTimer(
            callback: TimeoutSeeksAsync,
            dueTime: TimeSpan.FromMinutes(1),
            period: TimeSpan.FromMinutes(1)
        );

        _streamProvider = this.GetStreamProvider(Streaming.StreamProvider);
        _openSeekCreatedStream = _streamProvider.GetStream<OpenSeekCreatedEvent>(
            MatchmakingStreamConstants.OpenSeekCreatedStream
        );
        _openSeekRemovedStream = _streamProvider.GetStream<OpenSeekRemovedEvent>(
            MatchmakingStreamConstants.OpenSeekRemovedStream
        );

        var poolDirectoryGrain = GrainFactory.GetGrain<IPoolDirectoryGrain>(0);
        await poolDirectoryGrain.RegisterPoolAsync(_key);

        await base.OnActivateAsync(cancellationToken);
    }

    public override async Task OnDeactivateAsync(
        DeactivationReason reason,
        CancellationToken cancellationToken
    )
    {
        var poolDirectoryGrain = GrainFactory.GetGrain<IPoolDirectoryGrain>(0);
        await poolDirectoryGrain.UnregisterPoolAsync(_key);

        await base.OnDeactivateAsync(reason, cancellationToken);
    }
}
