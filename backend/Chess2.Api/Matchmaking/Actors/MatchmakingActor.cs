using Akka.Actor;
using Akka.Cluster.Sharding;

namespace Chess2.Api.Matchmaking.Actors;

public class MatchmakingActor : ReceiveActor
{
    public MatchmakingActor()
    {
        Receive<string>(test =>
        {
            Console.WriteLine(test);
        });
        Receive<ReceiveTimeout>(_ =>
        {
            Context.Parent.Tell(new Passivate(PoisonPill.Instance));
        });
    }

    public static Props PropsFor() => Props.Create(() => new MatchmakingActor());

    protected override void PreStart()
    {
        Context.SetReceiveTimeout(TimeSpan.FromSeconds(1));
    }
}
