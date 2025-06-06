using Akka.Actor;
using Akka.Cluster.Sharding;
using Chess2.Api.Game.DTOs;
using Chess2.Api.Game.Models;
using Chess2.Api.Game.Services;

namespace Chess2.Api.Game.Actors;

public record GameState(string PlayerWhite, string PlayerBlack, string PlayerToMove);

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
                PlayerWhite: startGame.UserId1,
                PlayerBlack: startGame.UserId2,
                PlayerToMove: startGame.UserId1
            );
            Sender.Tell(new GameEvents.GameStartedEvent());
            Become(Playing);
        });

        Receive<GameQueries.GetGameStatus>(_ =>
        {
            Sender.Tell(new GameEvents.GameStatusEvent(GameStatus.NotStarted), Self);
            Context.Parent.Tell(new Passivate(PoisonPill.Instance));
        });
    }

    private void Playing()
    {
        if (_gameState is null)
            throw new InvalidOperationException(
                $"Cannot become {nameof(Playing)} before setting {nameof(_gameState)}"
            );

        Receive<GameQueries.GetGameStatus>(_ =>
            Sender.Tell(new GameEvents.GameStatusEvent(GameStatus.OnGoing))
        );

        Receive<GameQueries.GetGameState>(getGameState =>
        {
            var gameStateDto = new GameStateDto(
                PlayerWhite: _gameState.PlayerWhite,
                PlayerBlack: _gameState.PlayerBlack,
                PlayerToMove: _gameState.PlayerToMove,
                Fen: _game.Fen,
                Moves: _game.Moves,
                LegalMoves: _game.LegalMoves
            );

            Sender.Tell(gameStateDto, Self);
        });
    }
}
