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

    private readonly TPool _pool;
    private readonly PoolKey _key;

    private readonly ILogger<AbstractMatchmakingGrain<TPool>> _logger;
    private readonly AppSettings _settings;
    private readonly IGameStarter _gameStarter;

    private IStreamProvider _streamProvider = null!;
    private IAsyncStream<SeekCreatedBroadcastEvent> _seekCreationStream = null!;

    public AbstractMatchmakingGrain(
        ILogger<AbstractMatchmakingGrain<TPool>> logger,
        IGameStarter gameStarter,
        IOptions<AppSettings> settings,
        TPool pool
    )
    {
        _key = PoolKey.FromGrainKey(this.GetPrimaryKeyString());

        _pool = pool;
        _logger = logger;
        _gameStarter = gameStarter;
        _settings = settings.Value;
    }

    public async Task<bool> TryCreateSeekAsync(Seeker seeker)
    {
        _logger.LogInformation("Received create seek from {UserId}", seeker.UserId);

        if (!_pool.TryAddSeek(seeker))
        {
            _logger.LogInformation("{UserId} already has a seek", seeker.UserId);
            return false;
        }

        await _seekCreationStream.OnNextAsync(new(seeker, _key));
        DelayDeactivation(TimeSpan.FromMinutes(5));

        return true;
    }

    public Task<bool> TryCancelSeekAsync(UserId userId)
    {
        _logger.LogInformation("Received cancel seek from {UserId}", userId);
        if (!_pool.RemoveSeek(userId))
        {
            _logger.LogInformation("No seek found for user {UserId}", userId);
            return Task.FromResult(false);
        }

        return Task.FromResult(true);
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

            SeekMatchedEvent matchedEvent = new(gameToken);
            await _streamProvider
                .GetStream<SeekMatchedEvent>(
                    MatchmakingStreamConstants.SeekMatchedStream,
                    MatchmakingStreamKey.MatchedStream(seeker1.UserId, _key)
                )
                .OnNextAsync(matchedEvent);
            await _streamProvider
                .GetStream<SeekMatchedEvent>(
                    MatchmakingStreamConstants.SeekMatchedStream,
                    MatchmakingStreamKey.MatchedStream(seeker2.UserId, _key)
                )
                .OnNextAsync(matchedEvent);
        }
    }

    public override Task OnActivateAsync(CancellationToken cancellationToken)
    {
        this.RegisterGrainTimer(
            callback: ExecuteWaveAsync,
            dueTime: TimeSpan.Zero,
            period: _settings.Game.MatchWaveEvery
        );
        _streamProvider = this.GetStreamProvider(Streaming.StreamProvider);
        _seekCreationStream = _streamProvider.GetStream<SeekCreatedBroadcastEvent>(
            MatchmakingStreamConstants.SeekCreationBoardcastStream
        );

        return base.OnActivateAsync(cancellationToken);
    }
}
