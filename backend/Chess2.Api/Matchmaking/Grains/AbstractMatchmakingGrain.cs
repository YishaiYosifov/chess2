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
    private IAsyncStream<OpenSeekBroadcastEvent> _seekCreationStream = null!;
    private IAsyncStream<OpenSeekEndedBroadcastEvent> _seekEndedStream = null!;

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

    public async Task AddSeekAsync(Seeker seeker)
    {
        DelayDeactivation(TimeSpan.FromMinutes(5));
        _logger.LogInformation("Received create seek from {UserId}", seeker.UserId);

        _pool.AddSeek(seeker);
        await _seekCreationStream.OnNextAsync(
            new OpenSeekBroadcastEvent(new SeekKey(seeker.UserId, _key), seeker)
        );
    }

    public async Task<bool> TryCancelSeekAsync(UserId userId)
    {
        _logger.LogInformation("Received cancel seek from {UserId}", userId);
        if (!_pool.RemoveSeek(userId))
        {
            _logger.LogInformation("No seek found for user {UserId}", userId);
            return false;
        }

        await NotifySeekEnded(userId);
        return true;
    }

    public Task<List<Seeker>> GetSeekersAsync()
    {
        return Task.FromResult(_pool.Seekers.ToList());
    }

    private async Task ExecuteWaveAsync()
    {
        var matches = _pool.CalculateMatches();
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

            await NotifySeekEnded(seeker1.UserId, gameToken);
            await NotifySeekEnded(seeker2.UserId, gameToken);
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
            _pool.RemoveSeek(seeker.UserId);
            await NotifySeekEnded(seeker.UserId);
        }

        if (_pool.SeekerCount > 0)
            DelayDeactivation(TimeSpan.FromMinutes(5));
        else
            DeactivateOnIdle();
    }

    private async Task NotifySeekEnded(UserId userId, string? gameToken = null)
    {
        await _streamProvider
            .GetStream<SeekEndedEvent>(
                MatchmakingStreamConstants.EndedStream,
                MatchmakingStreamKey.SeekStream(userId, _key)
            )
            .OnNextAsync(new(gameToken));
        await _seekEndedStream.OnNextAsync(
            new OpenSeekEndedBroadcastEvent(new SeekKey(userId, _key))
        );
    }

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
        _seekCreationStream = _streamProvider.GetStream<OpenSeekBroadcastEvent>(
            MatchmakingStreamConstants.OpenSeekBoardcastStream
        );
        _seekEndedStream = _streamProvider.GetStream<OpenSeekEndedBroadcastEvent>(
            MatchmakingStreamConstants.OpenSeekEndedBoardcastStream
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
