using Chess2.Api.Infrastructure;
using Chess2.Api.LiveGame.Services;
using Chess2.Api.Matchmaking.Errors;
using Chess2.Api.Matchmaking.Models;
using Chess2.Api.Matchmaking.Services.Pools;
using Chess2.Api.Matchmaking.Stream;
using Chess2.Api.Shared.Models;
using Chess2.Api.Profile.Models;
using ErrorOr;
using Microsoft.Extensions.Options;
using Orleans.Streams;
using Orleans.Utilities;

namespace Chess2.Api.Matchmaking.Grains;

[Alias("Chess2.Api.Matchmaking.Grains.IMatchmakingGrain")]
public interface IMatchmakingGrain : IGrainWithStringKey
{
    [Alias("AddSeekAsync")]
    Task AddSeekAsync(Seeker seeker, ISeekObserver observer);

    [Alias("CancelSeekAsync")]
    Task<bool> TryCancelSeekAsync(UserId userId);

    [Alias("GetMatchingSeekersForAsync")]
    Task<List<Seeker>> GetSeekersAsync();

    [Alias("MatchWithSeeker")]
    Task<ErrorOr<string>> MatchWithSeekerAsync(Seeker seeker, UserId matchWith);
}

[Alias("Chess2.Api.Matchmaking.Grains.IMatchmakingGrain`1")]
public interface IMatchmakingGrain<TPool> : IMatchmakingGrain
    where TPool : IMatchmakingPool;

public class MatchmakingGrain<TPool> : Grain, IMatchmakingGrain<TPool>
    where TPool : IMatchmakingPool
{
    public const int WaveTimer = 0;
    public const int TimeoutTimer = 1;

    private readonly TPool _pool;
    private readonly PoolKey _key;

    private readonly ILogger<MatchmakingGrain<TPool>> _logger;
    private readonly IGameStarter _gameStarter;
    private readonly TimeProvider _timeProvider;
    private readonly LobbySettings _settings;

    private IStreamProvider _streamProvider = null!;
    private IAsyncStream<OpenSeekCreatedEvent> _openSeekCreatedStream = null!;
    private IAsyncStream<OpenSeekRemovedEvent> _openSeekRemovedStream = null!;

    private readonly ObserverManager<UserId, ISeekObserver> _seekObservers;
    private readonly Dictionary<UserId, Seeker> _pendingSeekBroadcast = [];
    private readonly TimeSpan _seekTimeout = TimeSpan.FromMinutes(5);

    public MatchmakingGrain(
        ILogger<MatchmakingGrain<TPool>> logger,
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

        _seekObservers = new(_seekTimeout, _logger);
    }

    public Task AddSeekAsync(Seeker seeker, ISeekObserver handler)
    {
        _logger.LogInformation("Received create seek from {UserId}", seeker.UserId);

        _pool.AddSeeker(seeker);
        _pendingSeekBroadcast[seeker.UserId] = seeker;
        _seekObservers.Subscribe(seeker.UserId, handler);

        return Task.CompletedTask;
    }

    public async Task<bool> TryCancelSeekAsync(UserId userId)
    {
        _logger.LogInformation("Received cancel seek from {UserId}", userId);
        var result = await TryRemoveSeekerAsync(userId);
        return result;
    }

    public Task<List<Seeker>> GetSeekersAsync() => Task.FromResult(_pool.Seekers.ToList());

    public async Task<ErrorOr<string>> MatchWithSeekerAsync(Seeker seeker, UserId matchWith)
    {
        if (
            !_pool.TryGetSeeker(matchWith, out var matchWithSeeker)
            || !_seekObservers.Observers.TryGetValue(matchWith, out var matchWithObserver)
        )
            return MatchmakingErrors.SeekNotFound;

        if (!seeker.IsCompatibleWith(matchWithSeeker) || !matchWithSeeker.IsCompatibleWith(seeker))
            return MatchmakingErrors.RequestedSeekerNotCompatible;

        if (!await matchWithObserver.TryReserveSeekAsync(_key))
            return MatchmakingErrors.SeekNotFound;

        try
        {
            var gameToken = await _gameStarter.StartGameAsync(seeker.UserId, matchWith, _key);

            await matchWithObserver.SeekMatchedAsync(gameToken, _key);
            await BroadcastSeekRemoval(matchWith);
            _pool.RemoveSeeker(matchWith);
            return gameToken;
        }
        finally
        {
            await matchWithObserver.ReleaseReservationAsync(_key);
        }
    }

    private async Task ExecuteWaveAsync()
    {
        var matches = _pool.CalculateMatches();

        List<UserId> removedUserSeeks = [];
        foreach (var (seeker1, seeker2) in matches)
        {
            _logger.LogInformation("Found match for {User1} with {User2}", seeker1, seeker2);

            if (
                !_seekObservers.Observers.TryGetValue(seeker1.UserId, out var seeker1Handler)
                || !_seekObservers.Observers.TryGetValue(seeker2.UserId, out var seeker2Handler)
            )
            {
                _logger.LogWarning(
                    "Cannot start game for {User1} and {User2} because one of them has no observer",
                    seeker1.UserId,
                    seeker2.UserId
                );
                continue;
            }

            var didGameStart = await StartGameAsync(
                seeker1,
                seeker1Handler,
                seeker2,
                seeker2Handler
            );
            if (!didGameStart)
                continue;

            _pool.RemoveSeeker(seeker1.UserId);
            _pool.RemoveSeeker(seeker2.UserId);
            if (!_pendingSeekBroadcast.Remove(seeker1.UserId))
                removedUserSeeks.Add(seeker1.UserId);
            if (!_pendingSeekBroadcast.Remove(seeker2.UserId))
                removedUserSeeks.Add(seeker2.UserId);
        }

        if (removedUserSeeks.Count > 0)
        {
            await _openSeekRemovedStream.OnNextBatchAsync(
                removedUserSeeks.Select(userId => new OpenSeekRemovedEvent(userId, _key))
            );
        }

        // broadcast new seeks that survived the wave
        if (_pendingSeekBroadcast.Count > 0)
        {
            await _openSeekCreatedStream.OnNextBatchAsync(
                _pendingSeekBroadcast.Values.Select(seeker => new OpenSeekCreatedEvent(
                    seeker,
                    _key
                ))
            );
            _pendingSeekBroadcast.Clear();
        }
    }

    private async Task<bool> StartGameAsync(
        Seeker seeker1,
        ISeekObserver seeker1Observer,
        Seeker seeker2,
        ISeekObserver seeker2Observer
    )
    {
        try
        {
            var seeker1Reserved = await seeker1Observer.TryReserveSeekAsync(_key);
            var seeker2Reserved = await seeker2Observer.TryReserveSeekAsync(_key);
            if (!seeker1Reserved || !seeker2Reserved)
                return false;

            var gameToken = await _gameStarter.StartGameAsync(seeker1.UserId, seeker2.UserId, _key);

            await seeker1Observer.SeekMatchedAsync(gameToken, _key);
            await seeker2Observer.SeekMatchedAsync(gameToken, _key);
            return true;
        }
        finally
        {
            await seeker1Observer.ReleaseReservationAsync(_key);
            await seeker2Observer.ReleaseReservationAsync(_key);
        }
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
            await TryRemoveSeekerAsync(seeker.UserId);
        }
        _seekObservers.ClearExpired();

        if (_pool.SeekerCount > 0)
            DelayDeactivation(TimeSpan.FromMinutes(5));
        else
            DeactivateOnIdle();
    }

    private async Task<bool> TryRemoveSeekerAsync(UserId userId)
    {
        if (!_pool.RemoveSeeker(userId))
            return false;

        if (_seekObservers.Observers.TryGetValue(userId, out var observer))
            await observer.SeekRemovedAsync(_key);

        await BroadcastSeekRemoval(userId);

        _seekObservers.Unsubscribe(userId);
        return true;
    }

    private async Task BroadcastSeekRemoval(UserId userId)
    {
        // if the seeker is pending broadcast, it means it hasn't been broadcasted yet
        // so no point broadcasting it ended
        if (!_pendingSeekBroadcast.Remove(userId))
        {
            await _openSeekRemovedStream.OnNextAsync(new OpenSeekRemovedEvent(userId, _key));
        }
    }

    public override async Task OnActivateAsync(CancellationToken cancellationToken)
    {
        _streamProvider = this.GetStreamProvider(Streaming.StreamProvider);
        _openSeekCreatedStream = _streamProvider.GetStream<OpenSeekCreatedEvent>(
            MatchmakingStreamConstants.OpenSeekCreatedStream
        );
        _openSeekRemovedStream = _streamProvider.GetStream<OpenSeekRemovedEvent>(
            MatchmakingStreamConstants.OpenSeekRemovedStream
        );

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
