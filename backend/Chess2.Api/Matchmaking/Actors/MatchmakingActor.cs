using Akka.Actor;
using Akka.Cluster.Sharding;
using Akka.Event;
using Chess2.Api.Matchmaking.Models;

namespace Chess2.Api.Matchmaking.Actors;

public class MatchmakingActor : ReceiveActor
{
    // keep the order they were added so players that have been waiting the longest get matched first
    private readonly OrderedDictionary<string, SeekInfo> _seekers = [];
    private readonly ILoggingAdapter _logger = Context.GetLogger();

    public MatchmakingActor()
    {
        Receive<MatchmakingCommands.CreateSeek>(HandleCreateSeek);
        Receive<MatchmakingCommands.CancelSeek>(HandleCancelSeek);

        Receive<ReceiveTimeout>(_ => HandleTimeout());
    }

    public static Props PropsFor() => Props.Create(() => new MatchmakingActor());

    private void HandleCreateSeek(MatchmakingCommands.CreateSeek createSeek)
    {
        _logger.Info(
            "Received seek from {0} with rating {1}",
            createSeek.UserId,
            createSeek.Rating
        );

        var seek = new SeekInfo(createSeek.Rating);
        _seekers[createSeek.UserId] = seek;
    }

    private void HandleCancelSeek(MatchmakingCommands.CancelSeek cancelSeek)
    {
        _logger.Info("Received cancel seek from {0}", cancelSeek.UserId);
        _seekers.Remove(cancelSeek.UserId);
    }

    private void HandleTimeout()
    {
        if (_seekers.Count != 0)
            return;

        Context.Parent.Tell(new Passivate(PoisonPill.Instance));
    }

    protected override void PreStart()
    {
        Context.SetReceiveTimeout(TimeSpan.FromSeconds(1));
    }
}
