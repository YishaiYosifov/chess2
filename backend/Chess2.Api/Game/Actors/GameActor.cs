using Akka.Actor;
using Akka.Cluster.Sharding;
using Akka.Event;
using Chess2.Api.Game.Errors;
using Chess2.Api.Game.Models;
using Chess2.Api.Game.Services;
using Chess2.Api.GameLogic.Extensions;
using Chess2.Api.GameLogic.Models;
using Chess2.Api.Shared.Extensions;

namespace Chess2.Api.Game.Actors;

public class GameActor : ReceiveActor
{
    private readonly string _token;

    private readonly IGameCore _gameCore;
    private readonly IPlayerRoster _players;
    private readonly ILoggingAdapter _logger = Context.GetLogger();

    private TimeControlSettings _timeControl;

    public GameActor(string token, IGameCore game, IPlayerRoster playerRoster)
    {
        _token = token;
        _gameCore = game;
        _players = playerRoster;
        Become(WaitingForStart);
    }

    private void WaitingForStart()
    {
        Receive<GameCommands.StartGame>(HandleStartGame);

        Receive<GameQueries.GetGameStatus>(_ =>
        {
            Sender.Tell(new GameEvents.GameStatusEvent(GameStatus.NotStarted), Self);
            Context.Parent.Tell(new Passivate(PoisonPill.Instance));
        });
    }

    private void HandleStartGame(GameCommands.StartGame startGame)
    {
        _players.InitializePlayers(startGame.WhiteId, startGame.BlackId);
        _gameCore.InitializeGame();
        _timeControl = startGame.TimeControl;

        Sender.Tell(new GameEvents.GameStartedEvent());
        Become(Playing);
    }

    private void Playing()
    {
        Receive<GameQueries.GetGameStatus>(_ =>
            Sender.Tell(new GameEvents.GameStatusEvent(GameStatus.OnGoing))
        );

        Receive<GameQueries.GetGameState>(getGameState => HandleGetGameState(getGameState));

        Receive<GameCommands.EndGame>(HandleEndGame);

        Receive<GameCommands.MovePiece>(HandleMovePiece);
    }

    private void HandleGetGameState(GameQueries.GetGameState getGameState)
    {
        if (!_players.TryGetPlayerById(getGameState.ForUserId, out var player))
        {
            _logger.Warning(
                "Could not find player {0} when trying to get state for game {1}",
                getGameState.ForUserId,
                _token
            );
            Sender.ReplyWithErrorOr<GameEvents.GameStateEvent>(GameErrors.PlayerInvalid);
            return;
        }

        var gameState = GetGameStateForPlayer(player);
        Sender.ReplyWithErrorOr(new GameEvents.GameStateEvent(gameState));
    }

    private void HandleEndGame(GameCommands.EndGame endGame)
    {
        if (!_players.TryGetPlayerById(endGame.UserId, out var player))
        {
            _logger.Warning(
                "Could not find player {0} when trying to end game {1}",
                endGame.UserId,
                _token
            );
            Sender.ReplyWithErrorOr<GameEvents.GameEnded>(GameErrors.PlayerInvalid);
            return;
        }

        var state = GetGameStateForPlayer(player);
        GameResult result;

        var isAbort = _gameCore.MoveNumber <= 2;
        if (isAbort)
        {
            result = GameResult.Aborted;
        }
        else
        {
            var winnerColor = player.Color.Invert();
            result = winnerColor switch
            {
                GameColor.White => GameResult.WhiteWin,
                GameColor.Black => GameResult.BlackWin,
                _ => throw new InvalidOperationException($"Invalid Color {winnerColor}?"),
            };
        }

        _logger.Info("Game {0} ended by user {1}. Result: {2}", _token, endGame.UserId, result);
        Sender.ReplyWithErrorOr(new GameEvents.GameEnded(result, state));
        Context.Parent.Tell(new Passivate(PoisonPill.Instance));
    }

    private void HandleMovePiece(GameCommands.MovePiece movePiece)
    {
        if (
            !_players.TryGetPlayerByColor(_gameCore.SideToMove, out var currentPlayer)
            || currentPlayer.UserId != movePiece.UserId
        )
        {
            _logger.Warning(
                "User {0} attmpted to move a piece, but their id doesn't match the current player {1}",
                movePiece.UserId,
                currentPlayer?.UserId
            );
            Sender.ReplyWithErrorOr<GameEvents.PieceMoved>(GameErrors.PlayerInvalid);
            return;
        }

        var moveResult = _gameCore.MakeMove(movePiece.From, movePiece.To);
        if (moveResult.IsError)
        {
            Sender.ReplyWithErrorOr<GameEvents.PieceMoved>(moveResult.Errors);
            return;
        }
        var encodedMove = moveResult.Value;

        Sender.ReplyWithErrorOr(
            new GameEvents.PieceMoved(
                Move: encodedMove,
                WhiteLegalMoves: _gameCore.GetEncodedLegalMovesFor(GameColor.White),
                WhiteId: _players.WhitePlayer.UserId,
                BlackLegalMoves: _gameCore.GetEncodedLegalMovesFor(GameColor.Black),
                BlackId: _players.BlackPlayer.UserId,
                SideToMove: _gameCore.SideToMove,
                MoveNumber: _gameCore.MoveNumber
            )
        );
    }

    private GameState GetGameStateForPlayer(GamePlayer player)
    {
        var legalMoves = _gameCore.GetEncodedLegalMovesFor(player.Color);
        var gameState = new GameState(
            WhitePlayer: _players.WhitePlayer,
            BlackPlayer: _players.BlackPlayer,
            SideToMove: _gameCore.SideToMove,
            Fen: _gameCore.Fen,
            MoveHistory: _gameCore.EncodedMoveHistory,
            LegalMoves: legalMoves,
            TimeControl: _timeControl
        );
        return gameState;
    }
}
