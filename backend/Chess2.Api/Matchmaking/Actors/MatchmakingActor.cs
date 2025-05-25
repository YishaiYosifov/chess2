using Akka.Actor;
using Akka.Cluster.Sharding;
using Akka.Event;
using Chess2.Api.Matchmaking.Models;
using Chess2.Api.Matchmaking.Services.Pools;
using Chess2.Api.Shared.Models;
using Microsoft.Extensions.Options;

namespace Chess2.Api.Matchmaking.Actors;

public abstract class MatchmakingActor<TPool> : ReceiveActor, IWithTimers where TPool : IMatchmakingPool
{
    protected readonly ILoggingAdapter _logger = Context.GetLogger();
    protected readonly TPool _pool;
    private readonly AppSettings _settings;

    public ITimerScheduler Timers { get; set; } = null!;

    public MatchmakingActor(
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

public class RatedMatchmakingActor : MatchmakingActor<IRatedMatchmakingPool>
{
    public RatedMatchmakingActor(IOptions<AppSettings> settings, IRatedMatchmakingPool pool, ITimerScheduler? timerScheduler = null) : base(settings, pool, timerScheduler)
    {
        Receive<MatchmakingCommands.CreateRatedSeek>(HandleCreateSeek);
    }

    private void HandleCreateSeek(MatchmakingCommands.CreateRatedSeek createSeek)
    {
        _logger.Info(
            "Received seek from {0} with rating {1}",
            createSeek.UserId,
            createSeek.Rating
        );

        _pool.AddSeek(createSeek.UserId, createSeek.Rating);
    }
}

public class CasualMatchmakingActor : MatchmakingActor<ICasualMatchmakingPool>
{
    public CasualMatchmakingActor(IOptions<AppSettings> settings, ICasualMatchmakingPool pool, ITimerScheduler? timerScheduler = null) : base(settings, pool, timerScheduler)
    {
        Receive<MatchmakingCommands.CreateCasualSeek>(HandleCreateSeek);
    }

    private void HandleCreateSeek(MatchmakingCommands.CreateCasualSeek createSeek)
    {
        _logger.Info(
            "Received casual seek from {0}",
            createSeek.UserId
        );

        _pool.AddSeek(createSeek.UserId);
    }
}