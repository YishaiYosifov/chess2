using Chess2.Api.LiveGame.Services;
using Chess2.Api.Matchmaking.Models;
using Chess2.Api.Matchmaking.Services.Pools;
using Chess2.Api.PlayerSession.Actors;
using Chess2.Api.Shared.Models;
using Microsoft.Extensions.Options;
using Orleans.Utilities;

namespace Chess2.Api.Matchmaking.Grains;

[Alias("Chess2.Api.Matchmaking.Grains.IMatchmakingGrain")]
public interface IMatchmakingGrain : IGrainWithStringKey
{
    [Alias("CancelSeekAsync")]
    Task CancelSeekAsync(string userId);
}

public abstract class AbstractMatchmakinGrain<TPool> : Grain, IMatchmakingGrain, IGrain
    where TPool : IMatchmakingPool
{
    protected ILogger<AbstractMatchmakinGrain<TPool>> Logger { get; }
    protected TPool Pool { get; }
    protected abstract bool IsRated { get; }

    private readonly PoolKey _key;
    private readonly AppSettings _settings;
    private readonly ObserverManager<string, IPlayerSessionGrain> _subsManager;
    private readonly ILiveGameService _liveGameService;

    private IDisposable? _waveTimer;

    public AbstractMatchmakinGrain(
        ILogger<AbstractMatchmakinGrain<TPool>> logger,
        ILiveGameService liveGameService,
        IOptions<AppSettings> settings,
        TPool pool
    )
    {
        _key = PoolKey.FromGrainKey(this.GetPrimaryKeyString());

        Pool = pool;
        Logger = logger;
        _subsManager = new(TimeSpan.FromMinutes(5), Logger);
        _liveGameService = liveGameService;
        _settings = settings.Value;
    }

    protected bool TrySubscribeSeeker(string userId, IPlayerSessionGrain playerSessionGrain)
    {
        if (Pool.HasSeek(userId))
            return false;

        _subsManager.Subscribe(userId, playerSessionGrain);
        return true;
    }

    public Task CancelSeekAsync(string userId)
    {
        Logger.LogInformation("Received cancel seek from {UserId}", userId);
        if (!Pool.RemoveSeek(userId))
        {
            Logger.LogWarning("No seek found for user {UserId}", userId);
            return Task.CompletedTask;
        }

        _subsManager.Unsubscribe(userId);
        return Task.CompletedTask;
    }

    public async Task OnMatchWaveAsync()
    {
        var matches = Pool.CalculateMatches();
        foreach (var (seeker1, seeker2) in matches)
        {
            Logger.LogInformation("Found match for {User1} with {User2}", seeker1, seeker2);

            var gameToken = await _liveGameService.StartGameAsync(
                seeker1,
                seeker2,
                _key.TimeControl,
                IsRated
            );

            if (_subsManager.Observers.TryGetValue(seeker1, out var seeker1Ref))
                await seeker1Ref.MatchFoundAsync(gameToken, _key);
            if (_subsManager.Observers.TryGetValue(seeker2, out var seeker2Ref))
                await seeker2Ref.MatchFoundAsync(gameToken, _key);
        }
    }

    public override Task OnActivateAsync(CancellationToken cancellationToken)
    {
        _waveTimer = this.RegisterGrainTimer(
            callback: OnMatchWaveAsync,
            dueTime: TimeSpan.Zero,
            period: _settings.Game.MatchWaveEvery
        );

        return base.OnActivateAsync(cancellationToken);
    }
}
