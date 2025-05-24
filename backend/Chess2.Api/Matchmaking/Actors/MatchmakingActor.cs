using Akka.Actor;
using Akka.Cluster.Sharding;
using Akka.Event;
using Chess2.Api.Matchmaking.Models;
using Chess2.Api.Matchmaking.Services;
using Chess2.Api.Shared.Models;
using Microsoft.Extensions.Options;

namespace Chess2.Api.Matchmaking.Actors;

public class MatchmakingActor : ReceiveActor, IWithTimers
{
    private readonly ILoggingAdapter _logger = Context.GetLogger();
    private readonly IMatchmakingPool _pool;
    private readonly AppSettings _settings;
    private readonly Dictionary<string, IActorRef> _subscribers = [];

    public ITimerScheduler Timers { get; set; } = null!;

    public MatchmakingActor(
        IOptions<AppSettings> settings,
        IMatchmakingPool pool,
        ITimerScheduler? timerScheduler = null
    )
    {
        // for unit testing
        if (timerScheduler is not null)
            Timers = timerScheduler;
        _settings = settings.Value;
        _pool = pool;

        Receive<MatchmakingCommands.CreateSeek>(HandleCreateSeek);
        Receive<MatchmakingCommands.CancelSeek>(HandleCancelSeek);
        Receive<MatchmakingCommands.MatchWave>(_ => HandleMatchWave());

        Receive<ReceiveTimeout>(_ => HandleTimeout());
    }

    private void HandleCreateSeek(MatchmakingCommands.CreateSeek createSeek)
    {
        _logger.Info(
            "Received seek from {0} with rating {1}",
            createSeek.UserId,
            createSeek.Rating
        );

        _subscribers[createSeek.UserId] = Sender;
        _pool.AddSeek(createSeek.UserId, createSeek.Rating);
    }

    private void HandleCancelSeek(MatchmakingCommands.CancelSeek cancelSeek)
    {
        _logger.Info("Received cancel seek from {0}", cancelSeek.UserId);
        _subscribers.Remove(cancelSeek.UserId);

        if (!_pool.RemoveSeek(cancelSeek.UserId))
            _logger.Warning("No seek found for user {0}", cancelSeek.UserId);
    }

    private void HandleMatchWave()
    {
        var matches = _pool.CalculateMatches();
        foreach (var (seeker1, seeker2) in matches)
        {
            _logger.Info("Found match for {0} with {1}", seeker1, seeker2);
            if (
                !_subscribers.Remove(seeker1, out var seeker1Subscriber)
                || !_subscribers.Remove(seeker2, out var seeker2Subscriber)
            )
            {
                _logger.Warning(
                    "One or both subscribers not found for match: {0} and {1}",
                    seeker1,
                    seeker2
                );

                continue;
            }

            _pool.RemoveSeek(seeker1);
            _pool.RemoveSeek(seeker2);
            seeker1Subscriber.Tell(new MatchmakingEvents.MatchFound(seeker2));
            seeker2Subscriber.Tell(new MatchmakingEvents.MatchFound(seeker1));
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
