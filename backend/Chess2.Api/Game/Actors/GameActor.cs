using Akka.Actor;

namespace Chess2.Api.Game.Actors;

public class GameActor : ReceiveActor
{
    private readonly string token;

    public GameActor(string token)
    {
        this.token = token;
    }
}
