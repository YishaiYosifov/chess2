using Akka.Actor;

namespace Chess2.Api.Player.Actors;

public class PlayerActor : ReceiveActor
{
    private readonly string _userId;

    public PlayerActor(string userId)
    {
        _userId = userId;
    }
}
