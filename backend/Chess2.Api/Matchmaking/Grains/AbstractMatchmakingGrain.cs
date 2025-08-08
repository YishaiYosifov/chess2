using Chess2.Api.LiveGame.Services;
using Chess2.Api.Matchmaking.Models;
using Chess2.Api.Matchmaking.Services.Pools;
using Chess2.Api.Shared.Models;
using Chess2.Api.Users.Models;
using Microsoft.Extensions.Options;
using Orleans.Utilities;

namespace Chess2.Api.Matchmaking.Grains;

[Alias("Chess2.Api.Matchmaking.Grains.IMatchmakingGrain")]
public interface IMatchmakingGrain : IGrainWithStringKey
{
    [Alias("TryCreateSeekAsync")]
    Task<bool> TryCreateSeekAsync(Seeker seeker, IMatchObserver seekGrain);

    [Alias("CancelSeekAsync")]
    Task CancelSeekAsync(UserId userId);
}

public abstract class AbstractMatchmakingGrain<TPool> : Grain, IMatchmakingGrain
    where TPool : IMatchmakingPool
{
    private readonly TPool _pool;
    private readonly PoolKey _key;

    private readonly ILogger<AbstractMatchmakingGrain<TPool>> _logger;
    private readonly AppSettings _settings;
    private readonly ObserverManager<UserId, IMatchObserver> _subsManager;
    private readonly ILiveGameService _liveGameService;

    public AbstractMatchmakingGrain(
        ILogger<AbstractMatchmakingGrain<TPool>> logger,
        ILiveGameService liveGameService,
        IOptions<AppSettings> settings,
        TPool pool
    )
    {
        _key = PoolKey.FromGrainKey(this.GetPrimaryKeyString());

        _pool = pool;
        _logger = logger;
        _subsManager = new(TimeSpan.FromMinutes(5), _logger);
        _liveGameService = liveGameService;
        _settings = settings.Value;
    }

    public Task<bool> TryCreateSeekAsync(Seeker seeker, IMatchObserver seekGrain)
    {
        _logger.LogInformation("Received create seek from {UserId}", seeker.UserId);
        if (_pool.HasSeek(seeker.UserId))
        {
            _logger.LogInformation("{UserId} already has a seek", seeker.UserId);
            return Task.FromResult(false);
        }

        if (!_pool.TryAddSeek(seeker))
        {
            _logger.LogWarning("Could not add {UserId} seek", seeker.UserId);
            return Task.FromResult(false);
        }

        _subsManager.Subscribe(seeker.UserId, seekGrain);
        return Task.FromResult(true);
    }

    public Task CancelSeekAsync(UserId userId)
    {
        _logger.LogInformation("Received cancel seek from {UserId}", userId);
        if (!_pool.RemoveSeek(userId))
        {
            _logger.LogInformation("No seek found for user {UserId}", userId);
            return Task.CompletedTask;
        }

        _subsManager.Unsubscribe(userId);
        return Task.CompletedTask;
    }

    public async Task OnMatchWaveAsync()
    {
        var matches = _pool.CalculateMatches();
        foreach (var (seeker1, seeker2) in matches)
        {
            _logger.LogInformation("Found match for {User1} with {User2}", seeker1, seeker2);

            var isRated = seeker1 is RatedSeeker && seeker2 is RatedSeeker;
            var gameToken = await _liveGameService.StartGameAsync(
                seeker1.UserId,
                seeker2.UserId,
                _key.TimeControl,
                isRated
            );

            if (_subsManager.Observers.TryGetValue(seeker1.UserId, out var player1Observer))
                await player1Observer.MatchFoundAsync(gameToken, _key);
            if (_subsManager.Observers.TryGetValue(seeker2.UserId, out var player2Observer))
                await player2Observer.MatchFoundAsync(gameToken, _key);
        }
    }

    private Task KeepPoolAlive()
    {
        if (_pool.SeekerCount > 0)
            DelayDeactivation(TimeSpan.FromMinutes(2));
        return Task.CompletedTask;
    }

    public override Task OnActivateAsync(CancellationToken cancellationToken)
    {
        this.RegisterGrainTimer(
            callback: OnMatchWaveAsync,
            dueTime: TimeSpan.Zero,
            period: _settings.Game.MatchWaveEvery
        );
        this.RegisterGrainTimer(
            callback: KeepPoolAlive,
            dueTime: TimeSpan.Zero,
            period: TimeSpan.FromMinutes(1)
        );

        return base.OnActivateAsync(cancellationToken);
    }
}
