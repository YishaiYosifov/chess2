using Akka.Actor;
using Akka.Persistence;

namespace Chess2.Api.Matchmaking.Actors;

public class MatchmakingActor : ReceivePersistentActor
{
    public MatchmakingActor(string persistenceId)
    {
        PersistenceId = persistenceId;
    }

    public override string PersistenceId { get; }

    public static Props PropsFor(string matchmakingId) =>
        Props.Create(() => new MatchmakingActor(matchmakingId));
}
