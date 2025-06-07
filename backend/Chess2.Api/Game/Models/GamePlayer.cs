using Akka.Actor;
using Chess2.Api.GameLogic.Models;

namespace Chess2.Api.Game.Models;

public class GamePlayer
{
    public required string UserId { get; init; }
    public required GameColor Color { get; init; }
    public required IActorRef PlayerActor { get; init; }
}
