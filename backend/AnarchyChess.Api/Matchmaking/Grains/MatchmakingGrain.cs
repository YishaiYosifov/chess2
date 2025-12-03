using AnarchyChess.Api.Game.Models;
using AnarchyChess.Api.Game.Services;
using AnarchyChess.Api.Infrastructure;
using AnarchyChess.Api.Matchmaking.Errors;
using AnarchyChess.Api.Matchmaking.Models;
using AnarchyChess.Api.Matchmaking.Services.Pools;
using AnarchyChess.Api.Profile.DTOs;
using AnarchyChess.Api.Profile.Models;
using AnarchyChess.Api.Shared.Models;
using ErrorOr;
using Microsoft.Extensions.Options;
using Orleans.Streams;

namespace AnarchyChess.Api.Matchmaking.Grains;

[Alias("AnarchyChess.Api.Matchmaking.Grains.IMatchmakingGrain")]
public interface IMatchmakingGrain : IGrainWithStringKey
{
    [Alias("AddSeekAsync")]
    Task AddSeekAsync(Seeker seeker, ISeekObserver observer, CancellationToken token = default);

    [Alias("CancelSeekAsync")]
    Task<bool> TryCancelSeekAsync(UserId userId, CancellationToken token = default);

    [Alias("MatchWithSeeker")]
    Task<ErrorOr<OngoingGame>> MatchWithSeekerAsync(
        Seeker seeker,
        UserId matchWith,
        CancellationToken token = default
    );
}

[Alias("AnarchyChess.Api.Matchmaking.Grains.IMatchmakingGrain`1")]
public interface IMatchmakingGrain<TPool> : IMatchmakingGrain
    where TPool : IMatchmakingPool;

[GenerateSerializer]
[Alias("AnarchyChess.Api.Matchmaking.Grains.MatchmakingGrainState`1")]
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

    private readonly PoolKey _poolKey;
    private readonly IPersistentState<MatchmakingGrainState<TPool>> _state;
    private readonly ILogger<MatchmakingGrain<TPool>> _logger;
    private readonly IGameStarterFactory _gameStarterFactory;
    private readonly TimeProvider _timeProvider;
    private readonly LobbySettings _settings;

    private IAsyncStream<OpenSeekCreatedEvent> _openSeekCreatedStream = null!;
    private IAsyncStream<OpenSeekRemovedEvent> _openSeekRemovedStream = null!;

    private readonly Dictionary<UserId, Seeker> _pendingSeekBroadcast = [];

    public MatchmakingGrain(
        [PersistentState(StateName)] IPersistentState<MatchmakingGrainState<TPool>> state,
        ILogger<MatchmakingGrain<TPool>> logger,
        IGameStarterFactory gameStarterFactory,
        IOptions<AppSettings> settings,
        TimeProvider timeProvider
    )
    {
        _poolKey = PoolKey.FromGrainKey(this.GetPrimaryKeyString());

        _state = state;
        _logger = logger;
        _gameStarterFactory = gameStarterFactory;
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

    public async Task<ErrorOr<OngoingGame>> MatchWithSeekerAsync(
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

        if (!await matchWithObserver.TryReserveSeekAsync(_poolKey))
            return MatchmakingErrors.SeekNotFound;

        try
        {
            var gameToken = await _gameStarterFactory.UseAsync(
                async (gameStarter, token) =>
                    await gameStarter.StartGameWithRandomColorsAsync(
                        seeker.UserId,
                        matchWith,
                        _poolKey,
                        GameSource.Matchmaking,
                        token: token
                    ),
                token
            );

            await matchWithObserver.SeekMatchedAsync(
                new OngoingGame(
                    gameToken,
                    _poolKey,
                    new MinimalProfile(seeker.UserId, seeker.UserName)
                ),
                token
            );
            await BroadcastSeekRemoval(matchWith);
            _state.State.Pool.RemoveSeeker(matchWith);
            await _state.WriteStateAsync(token);
            return new OngoingGame(
                gameToken,
                _poolKey,
                new MinimalProfile(matchWithSeeker.UserId, matchWithSeeker.UserName)
            );
        }
        finally
        {
            await matchWithObserver.ReleaseReservationAsync(_poolKey);
        }
    }

    private async Task ExecuteWaveAsync(CancellationToken token = default)
    {
        var pendingSeekRemovals = await _gameStarterFactory.UseAsync(ProcessWaveAsync, token);
        await _state.WriteStateAsync(token);

        if (pendingSeekRemovals.Count > 0)
        {
            await _openSeekRemovedStream.OnNextBatchAsync(
                pendingSeekRemovals.Select(userId => new OpenSeekRemovedEvent(userId, _poolKey))
            );
        }

        // broadcast new seeks that survived the wave
        if (_pendingSeekBroadcast.Count > 0)
        {
            await _openSeekCreatedStream.OnNextBatchAsync(
                _pendingSeekBroadcast.Values.Select(seeker => new OpenSeekCreatedEvent(
                    seeker,
                    _poolKey
                ))
            );
            _pendingSeekBroadcast.Clear();
        }
    }

    private async Task<List<UserId>> ProcessWaveAsync(
        IGameStarter gameStarter,
        CancellationToken token = default
    )
    {
        var matches = _state.State.Pool.CalculateMatches();
        List<UserId> pendingSeekRemovals = [];
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
                    seeker1,
                    seeker2
                );
                continue;
            }

            var didGameStart = await StartGameAsync(
                gameStarter,
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
                pendingSeekRemovals.Add(seeker1.UserId);
            if (!_pendingSeekBroadcast.Remove(seeker2.UserId))
                pendingSeekRemovals.Add(seeker2.UserId);
        }

        return pendingSeekRemovals;
    }

    private async Task<bool> StartGameAsync(
        IGameStarter gameStarter,
        Seeker seeker1,
        ISeekObserver seeker1Observer,
        Seeker seeker2,
        ISeekObserver seeker2Observer,
        CancellationToken token = default
    )
    {
        try
        {
            var seeker1Reserved = await seeker1Observer.TryReserveSeekAsync(_poolKey);
            var seeker2Reserved = await seeker2Observer.TryReserveSeekAsync(_poolKey);
            if (!seeker1Reserved || !seeker2Reserved)
                return false;

            var gameToken = await gameStarter.StartGameWithRandomColorsAsync(
                seeker1.UserId,
                seeker2.UserId,
                _poolKey,
                GameSource.Matchmaking,
                token: token
            );

            await seeker1Observer.SeekMatchedAsync(
                new OngoingGame(gameToken, _poolKey, new(seeker2.UserId, seeker2.UserName)),
                token
            );
            await seeker2Observer.SeekMatchedAsync(
                new OngoingGame(gameToken, _poolKey, new(seeker1.UserId, seeker1.UserName)),
                token
            );
            return true;
        }
        finally
        {
            await seeker1Observer.ReleaseReservationAsync(_poolKey);
            await seeker2Observer.ReleaseReservationAsync(_poolKey);
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
                _poolKey
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
            await observer.SeekRemovedAsync(_poolKey);

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
            await _openSeekRemovedStream.OnNextAsync(new OpenSeekRemovedEvent(userId, _poolKey));
        }
    }

    public override async Task OnActivateAsync(CancellationToken cancellationToken)
    {
        var streamProvider = this.GetStreamProvider(Streaming.StreamProvider);
        _openSeekCreatedStream = streamProvider.GetStream<OpenSeekCreatedEvent>(
            nameof(OpenSeekCreatedEvent)
        );
        _openSeekRemovedStream = streamProvider.GetStream<OpenSeekRemovedEvent>(
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
