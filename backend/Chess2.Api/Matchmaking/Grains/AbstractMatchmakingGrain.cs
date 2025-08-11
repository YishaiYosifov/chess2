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
    [Alias("TryCreateSeekAsync")]
    Task<bool> TryCreateSeekAsync(Seeker seeker);

    [Alias("CancelSeekAsync")]
    Task<bool> TryCancelSeekAsync(UserId userId);

    [Alias("GetMatchingSeekersForAsync")]
    Task<IEnumerable<Seeker>> GetMatchingSeekersForAsync(Seeker seeker);
}

public abstract class AbstractMatchmakingGrain<TPool> : Grain, IMatchmakingGrain
    where TPool : IMatchmakingPool
{
    public const int WaveTimer = 0;
    public const int ActivationTimer = 1;
    private readonly TimeSpan _seekTimeout = TimeSpan.FromMinutes(5);

    private readonly TPool _pool;
    private readonly PoolKey _key;

    private readonly ILogger<AbstractMatchmakingGrain<TPool>> _logger;
    private readonly AppSettings _settings;
    private readonly IGameStarter _gameStarter;
    private readonly TimeProvider _timeProvider;

    private IStreamProvider _streamProvider = null!;
    private IAsyncStream<SeekCreatedBroadcastEvent> _seekCreationStream = null!;

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
        _settings = settings.Value;
    }

    public async Task<bool> TryCreateSeekAsync(Seeker seeker)
    {
        DelayDeactivation(TimeSpan.FromMinutes(5));
        _logger.LogInformation("Received create seek from {UserId}", seeker.UserId);

        if (!_pool.AddSeek(seeker))
        {
            _logger.LogInformation("{UserId} already has a seek", seeker.UserId);
            return false;
        }

        await _seekCreationStream.OnNextAsync(new(seeker, _key));

        return true;
    }

    public async Task<bool> TryCancelSeekAsync(UserId userId)
    {
        _logger.LogInformation("Received cancel seek from {UserId}", userId);
        if (!_pool.RemoveSeek(userId))
        {
            _logger.LogInformation("No seek found for user {UserId}", userId);
            return false;
        }

        await NotifySeekInvalidatedAsync(userId, isMatched: false);
        return true;
    }

    public Task<IEnumerable<Seeker>> GetMatchingSeekersForAsync(Seeker seeker)
    {
        return Task.FromResult(
            _pool.Seekers.Where(match =>
                match.IsCompatibleWith(seeker) && seeker.IsCompatibleWith(match)
            )
        );
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

            await NotifySeekMatchedAsync(seeker1.UserId, gameToken);
            await NotifySeekMatchedAsync(seeker2.UserId, gameToken);

            await NotifySeekInvalidatedAsync(seeker1.UserId, isMatched: true);
            await NotifySeekInvalidatedAsync(seeker2.UserId, isMatched: true);
        }
    }

    private async Task TimeoutSeeksAsync()
    {
        var now = _timeProvider.GetUtcNow();
        foreach (var seeker in _pool.Seekers)
        {
            if (now - seeker.CreatedAt < _seekTimeout)
                continue;

            _pool.RemoveSeek(seeker.UserId);
            await NotifySeekInvalidatedAsync(seeker.UserId, isMatched: false);
        }

        if (_pool.SeekerCount > 0)
            DelayDeactivation(TimeSpan.FromMinutes(5));
        else
            DeactivateOnIdle();
    }

    private Task NotifySeekInvalidatedAsync(UserId userId, bool isMatched) =>
        _streamProvider
            .GetStream<SeekInvalidatedEvent>(
                MatchmakingStreamConstants.InvalidatedStream,
                MatchmakingStreamKey.SeekStream(userId, _key)
            )
            .OnNextAsync(new(isMatched));

    private Task NotifySeekMatchedAsync(UserId userId, string gameToken) =>
        _streamProvider
            .GetStream<SeekMatchedEvent>(
                MatchmakingStreamConstants.MatchedStream,
                MatchmakingStreamKey.SeekStream(userId, _key)
            )
            .OnNextAsync(new(gameToken));

    public override Task OnActivateAsync(CancellationToken cancellationToken)
    {
        this.RegisterGrainTimer(
            callback: ExecuteWaveAsync,
            dueTime: TimeSpan.Zero,
            period: _settings.Game.MatchWaveEvery
        );
        this.RegisterGrainTimer(
            callback: TimeoutSeeksAsync,
            dueTime: TimeSpan.FromMinutes(1),
            period: TimeSpan.FromMinutes(1)
        );

        _streamProvider = this.GetStreamProvider(Streaming.StreamProvider);
        _seekCreationStream = _streamProvider.GetStream<SeekCreatedBroadcastEvent>(
            MatchmakingStreamConstants.SeekCreationBoardcastStream
        );

        return base.OnActivateAsync(cancellationToken);
    }
}
