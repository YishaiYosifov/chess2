using Chess2.Api.Game.Models;
using Chess2.Api.Game.Services;
using Chess2.Api.Infrastructure;
using Chess2.Api.Matchmaking.Errors;
using Chess2.Api.Matchmaking.Models;
using Chess2.Api.Matchmaking.Services.Pools;
using Chess2.Api.Profile.Models;
using Chess2.Api.Shared.Models;
using ErrorOr;
using Microsoft.Extensions.Options;
using Orleans.Streams;

namespace Chess2.Api.Matchmaking.Grains;

[Alias("Chess2.Api.Matchmaking.Grains.IMatchmakingGrain")]
public interface IMatchmakingGrain : IGrainWithStringKey
{
    [Alias("AddSeekAsync")]
    Task AddSeekAsync(Seeker seeker, ISeekObserver observer, CancellationToken token = default);

    [Alias("CancelSeekAsync")]
    Task<bool> TryCancelSeekAsync(UserId userId, CancellationToken token = default);

    [Alias("MatchWithSeeker")]
    Task<ErrorOr<GameToken>> MatchWithSeekerAsync(
        Seeker seeker,
        UserId matchWith,
        CancellationToken token = default
    );
}

[Alias("Chess2.Api.Matchmaking.Grains.IMatchmakingGrain`1")]
public interface IMatchmakingGrain<TPool> : IMatchmakingGrain
    where TPool : IMatchmakingPool;

[GenerateSerializer]
[Alias("Chess2.Api.Matchmaking.Grains.MatchmakingGrainState`1")]
public class MatchmakingGrainState<TPool>
    where TPool : IMatchmakingPool, new()
{
    [Id(0)]
    public TPool Pool { get; } = new();

    [Id(1)]
    public Dictionary<UserId, ISeekObserver> SeekObservers { get; } = [];
}

public class MatchmakingGrain<TPool> : Grain, IMatchmakingGrain<TPool>
    where TPool : IMatchmakingPool, new()
{
    public const int WaveTimer = 0;
    public const int TimeoutTimer = 1;
    public const string StateName = "matchmaking";

    private readonly PoolKey _key;
    private readonly IPersistentState<MatchmakingGrainState<TPool>> _state;
    private readonly ILogger<MatchmakingGrain<TPool>> _logger;
    private readonly IGameStarter _gameStarter;
    private readonly TimeProvider _timeProvider;
    private readonly LobbySettings _settings;

    private IStreamProvider _streamProvider = null!;
    private IAsyncStream<OpenSeekCreatedEvent> _openSeekCreatedStream = null!;
    private IAsyncStream<OpenSeekRemovedEvent> _openSeekRemovedStream = null!;

    private readonly Dictionary<UserId, Seeker> _pendingSeekBroadcast = [];

    public MatchmakingGrain(
        [PersistentState(StateName, StorageNames.PlayerSessionState)]
            IPersistentState<MatchmakingGrainState<TPool>> state,
        ILogger<MatchmakingGrain<TPool>> logger,
        IGameStarter gameStarter,
        IOptions<AppSettings> settings,
        TimeProvider timeProvider
    )
    {
        _key = PoolKey.FromGrainKey(this.GetPrimaryKeyString());

        _state = state;
        _logger = logger;
        _gameStarter = gameStarter;
        _timeProvider = timeProvider;
        _settings = settings.Value.Lobby;
    }

    public async Task AddSeekAsync(
        Seeker seeker,
        ISeekObserver handler,
        CancellationToken token = default
    )
    {
        _logger.LogInformation("Received create seek from {UserId}", seeker.UserId);

        _state.State.Pool.AddSeeker(seeker);
        _state.State.SeekObservers[seeker.UserId] = handler;
        await _state.WriteStateAsync(token);

        _pendingSeekBroadcast[seeker.UserId] = seeker;
    }

    public async Task<bool> TryCancelSeekAsync(UserId userId, CancellationToken token = default)
    {
        _logger.LogInformation("Received cancel seek from {UserId}", userId);
        var result = await TryRemoveSeekerAsync(userId);
        await _state.WriteStateAsync(token);
        return result;
    }

    public async Task<ErrorOr<GameToken>> MatchWithSeekerAsync(
        Seeker seeker,
        UserId matchWith,
        CancellationToken token = default
    )
    {
        if (
            !_state.State.Pool.TryGetSeeker(matchWith, out var matchWithSeeker)
            || !_state.State.SeekObservers.TryGetValue(matchWith, out var matchWithObserver)
        )
            return MatchmakingErrors.SeekNotFound;

        if (!seeker.IsCompatibleWith(matchWithSeeker) || !matchWithSeeker.IsCompatibleWith(seeker))
            return MatchmakingErrors.RequestedSeekerNotCompatible;

        if (!await matchWithObserver.TryReserveSeekAsync(_key))
            return MatchmakingErrors.SeekNotFound;

        try
        {
            var gameToken = await _gameStarter.StartGameAsync(
                seeker.UserId,
                matchWith,
                _key,
                token
            );

            await matchWithObserver.SeekMatchedAsync(gameToken, _key, token);
            await BroadcastSeekRemoval(matchWith);
            _state.State.Pool.RemoveSeeker(matchWith);
            await _state.WriteStateAsync(token);
            return gameToken;
        }
        finally
        {
            await matchWithObserver.ReleaseReservationAsync(_key);
        }
    }

    private async Task ExecuteWaveAsync(CancellationToken token = default)
    {
        var matches = _state.State.Pool.CalculateMatches();

        List<UserId> removedUserSeeks = [];
        foreach (var (seeker1, seeker2) in matches)
        {
            _logger.LogInformation("Found match for {User1} with {User2}", seeker1, seeker2);

            if (
                !_state.State.SeekObservers.TryGetValue(seeker1.UserId, out var seeker1Handler)
                || !_state.State.SeekObservers.TryGetValue(seeker2.UserId, out var seeker2Handler)
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
                seeker2Handler,
                token
            );
            if (!didGameStart)
                continue;

            _state.State.Pool.RemoveSeeker(seeker1.UserId);
            _state.State.Pool.RemoveSeeker(seeker2.UserId);
            if (!_pendingSeekBroadcast.Remove(seeker1.UserId))
                removedUserSeeks.Add(seeker1.UserId);
            if (!_pendingSeekBroadcast.Remove(seeker2.UserId))
                removedUserSeeks.Add(seeker2.UserId);
        }

        await _state.WriteStateAsync(token);

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
        ISeekObserver seeker2Observer,
        CancellationToken token = default
    )
    {
        try
        {
            var seeker1Reserved = await seeker1Observer.TryReserveSeekAsync(_key);
            var seeker2Reserved = await seeker2Observer.TryReserveSeekAsync(_key);
            if (!seeker1Reserved || !seeker2Reserved)
                return false;

            var gameToken = await _gameStarter.StartGameAsync(
                seeker1.UserId,
                seeker2.UserId,
                _key,
                token
            );

            await seeker1Observer.SeekMatchedAsync(gameToken, _key, token);
            await seeker2Observer.SeekMatchedAsync(gameToken, _key, token);
            return true;
        }
        finally
        {
            await seeker1Observer.ReleaseReservationAsync(_key);
            await seeker2Observer.ReleaseReservationAsync(_key);
        }
    }

    private async Task TimeoutSeeksAsync(CancellationToken token = default)
    {
        var now = _timeProvider.GetUtcNow();
        var timedOutSeekers = _state
            .State.Pool.Seekers.Where(seeker => now - seeker.CreatedAt >= _settings.SeekLifetime)
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
        await _state.WriteStateAsync(token);
    }

    private async Task<bool> TryRemoveSeekerAsync(UserId userId)
    {
        if (!_state.State.Pool.RemoveSeeker(userId))
            return false;

        if (_state.State.SeekObservers.TryGetValue(userId, out var observer))
            await observer.SeekRemovedAsync(_key);

        await BroadcastSeekRemoval(userId);

        _state.State.SeekObservers.Remove(userId);
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
            nameof(OpenSeekCreatedEvent)
        );
        _openSeekRemovedStream = _streamProvider.GetStream<OpenSeekRemovedEvent>(
            nameof(OpenSeekRemovedEvent)
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

        await base.OnActivateAsync(cancellationToken);
    }
}
