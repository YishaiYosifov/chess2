using Akka.Actor;
using Chess2.Api.Game.Models;
using Chess2.Api.Game.Services;

namespace Chess2.Api.Game.Actors;

public record GameState(string UserWhite, string UserBlack, string PlayingUser);

public class GameActor : ReceiveActor
{
    private readonly string _token;
    private readonly IGame _game;
    private GameState? _gameState;

    public GameActor(string token, IGame game)
    {
        _token = token;
        _game = game;

        Become(WaitingForStart);
    }

    private void WaitingForStart()
    {
        Receive<GameCommands.StartGame>(startGame =>
        {
            _gameState = new(
                UserWhite: startGame.UserId1,
                UserBlack: startGame.UserId2,
                PlayingUser: startGame.UserId1
            );
            Become(Playing);
        });
    }

    private void Playing()
    {
        if (_gameState is null)
            throw new InvalidOperationException(
                $"Cannot become {nameof(Playing)} before setting {nameof(_gameState)}"
            );
    }
}
