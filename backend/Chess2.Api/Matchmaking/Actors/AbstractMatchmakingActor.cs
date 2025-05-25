using Akka.Actor;
using Akka.Cluster.Sharding;
using Akka.Event;
using Chess2.Api.Matchmaking.Models;
using Chess2.Api.Matchmaking.Services.Pools;
using Chess2.Api.Shared.Models;
using Microsoft.Extensions.Options;

namespace Chess2.Api.Matchmaking.Actors;

public abstract class MatchmakingActor : ReceiveActor;

public abstract class AbstractMatchmakingActor<TPool> : MatchmakingActor, IWithTimers where TPool : IMatchmakingPool
{
    protected readonly ILoggingAdapter _logger = Context.GetLogger();
    protected readonly TPool _pool;
    private readonly AppSettings _settings;

    public ITimerScheduler Timers { get; set; } = null!;

    public AbstractMatchmakingActor(
        IOptions<AppSettings> settings,
        TPool pool,
        ITimerScheduler? timerScheduler = null
    )
    {
        // for unit testing
        if (timerScheduler is not null)
            Timers = timerScheduler;
        _settings = settings.Value;
        _pool = pool;

        Receive<MatchmakingCommands.CancelSeek>(HandleCancelSeek);
        Receive<MatchmakingCommands.MatchWave>(_ => HandleMatchWave());

        Receive<ReceiveTimeout>(_ => HandleTimeout());
    }

    private void HandleCancelSeek(MatchmakingCommands.CancelSeek cancelSeek)
    {
        _logger.Info("Received cancel seek from {0}", cancelSeek.UserId);

        if (!_pool.RemoveSeek(cancelSeek.UserId))
            _logger.Warning("No seek found for user {0}", cancelSeek.UserId);
    }

    private void HandleMatchWave()
    {
        var matches = _pool.CalculateMatches();
        foreach (var (seeker1, seeker2) in matches)
        {
            _logger.Info("Found match for {0} with {1}", seeker1, seeker2);
            // TODO: start game
        }
    }

    private void HandleTimeout()
    {
        if (_pool.SeekerCount != 0)
            return;

        Context.Parent.Tell(new Passivate(PoisonPill.Instance));
        _logger.Info("No seekers left, passivating actor");
    }

    protected override void PreStart()
    {
        Timers.StartPeriodicTimer(
            "wave",
            new MatchmakingCommands.MatchWave(),
            _settings.Game.MatchWaveEvery
        );
        Context.SetReceiveTimeout(TimeSpan.FromSeconds(30));
    }
}
