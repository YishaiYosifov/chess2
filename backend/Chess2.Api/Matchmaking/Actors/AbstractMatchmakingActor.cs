using Akka.Actor;
using Akka.Cluster.Sharding;
using Akka.Event;
using Chess2.Api.Matchmaking.Models;
using Chess2.Api.Matchmaking.Services.Pools;
using Chess2.Api.Shared.Models;
using Microsoft.Extensions.Options;

namespace Chess2.Api.Matchmaking.Actors;

public abstract class MatchmakingActor : ReceiveActor;

public abstract class AbstractMatchmakingActor<TPool> : MatchmakingActor, IWithTimers
    where TPool : IMatchmakingPool
{
    protected ILoggingAdapter Logger { get; } = Context.GetLogger();
    protected TPool Pool { get; }

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
        Pool = pool;

        Receive<ICreateSeekCommand>(HandleCreateSeek);
        Receive<MatchmakingCommands.CancelSeek>(HandleCancelSeek);
        Receive<MatchmakingCommands.MatchWave>(_ => HandleMatchWave());

        Receive<ReceiveTimeout>(_ => HandleTimeout());
    }

    protected abstract bool EnterPool(ICreateSeekCommand createSeek);
    
    private void HandleCreateSeek(ICreateSeekCommand createSeek)
    {
        if (!EnterPool(createSeek))
            return;

        Context.WatchWith(Sender, new MatchmakingCommands.CancelSeek(createSeek.UserId, createSeek.PoolInfo));
        Context.System.EventStream.Publish(new MatchmakingBroadcasts.SeekCreated(createSeek.UserId));
    }

    private void HandleCancelSeek(MatchmakingCommands.CancelSeek cancelSeek)
    {
        Logger.Info("Received cancel seek from {0}", cancelSeek.UserId);
        if (!Pool.RemoveSeek(cancelSeek.UserId))
        {
            Logger.Warning("No seek found for user {0}", cancelSeek.UserId);
            return;
        }

        Context.Unwatch(Sender);
        Context.System.EventStream.Publish(new MatchmakingBroadcasts.SeekCanceled(cancelSeek.UserId));
    }

    private void HandleMatchWave()
    {
        var matches = Pool.CalculateMatches();
        foreach (var (seeker1, seeker2) in matches)
        {
            Logger.Info("Found match for {0} with {1}", seeker1, seeker2);
            // TODO: start game
        }
    }

    private void HandleTimeout()
    {
        if (Pool.SeekerCount != 0)
            return;

        Context.Parent.Tell(new Passivate(PoisonPill.Instance));
        Logger.Info("No seekers left, passivating actor");
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
