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

public record PlayerRoster(
    GamePlayer WhitePlayer,
    GamePlayer BlackPlayer,
    IReadOnlyDictionary<string, GamePlayer> IdToPlayer,
    IReadOnlyDictionary<GameColor, GamePlayer> ColorToPlayer
);

public class GameActor : ReceiveActor
{
    private readonly string _token;

    private readonly IGameCore _gameCore;
    private readonly ILoggingAdapter _logger = Context.GetLogger();

    public GameActor(string token, IGameCore game)
    {
        _token = token;
        _gameCore = game;

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
        var playerWhite = new GamePlayer(UserId: startGame.WhiteId, Color: GameColor.White);
        var playerBlack = new GamePlayer(UserId: startGame.BlackId, Color: GameColor.Black);
        var idToPlayer = new Dictionary<string, GamePlayer>()
        {
            [playerWhite.UserId] = playerWhite,
            [playerBlack.UserId] = playerBlack,
        };
        var colorToPlayer = new Dictionary<GameColor, GamePlayer>()
        {
            [GameColor.White] = playerWhite,
            [GameColor.Black] = playerBlack,
        };

        var players = new PlayerRoster(
            WhitePlayer: playerWhite,
            BlackPlayer: playerBlack,
            IdToPlayer: idToPlayer.AsReadOnly(),
            ColorToPlayer: colorToPlayer.AsReadOnly()
        );
        _gameCore.InitializeGame();

        Sender.Tell(new GameEvents.GameStartedEvent());
        Become(() => Playing(players, startGame.TimeControl));
    }

    private void Playing(PlayerRoster players, TimeControlSettings timeControl)
    {
        Receive<GameQueries.GetGameStatus>(_ =>
            Sender.Tell(new GameEvents.GameStatusEvent(GameStatus.OnGoing))
        );

        Receive<GameQueries.GetGameState>(getGameState =>
            HandleGetGameState(getGameState, players, timeControl)
        );

        Receive<GameCommands.EndGame>(endGame => HandleEndGame(endGame, players, timeControl));

        Receive<GameCommands.MovePiece>(movePiece => HandleMovePiece(movePiece, players));
    }

    private void HandleGetGameState(
        GameQueries.GetGameState getGameState,
        PlayerRoster players,
        TimeControlSettings timeControl
    )
    {
        if (!players.IdToPlayer.TryGetValue(getGameState.ForUserId, out var player))
        {
            _logger.Warning(
                "Could not find player {0} when trying to get state for game {1}",
                getGameState.ForUserId,
                _token
            );
            Sender.ReplyWithErrorOr<GameEvents.GameStateEvent>(GameErrors.PlayerInvalid);
            return;
        }

        var gameState = GetGameStateForPlayer(players, player, timeControl);
        Sender.ReplyWithErrorOr(new GameEvents.GameStateEvent(gameState));
    }

    private void HandleEndGame(
        GameCommands.EndGame endGame,
        PlayerRoster players,
        TimeControlSettings timeControl
    )
    {
        if (!players.IdToPlayer.TryGetValue(endGame.UserId, out var player))
        {
            _logger.Warning(
                "Could not find player {0} when trying to end game {1}",
                endGame.UserId,
                _token
            );
            Sender.ReplyWithErrorOr<GameEvents.GameEnded>(GameErrors.PlayerInvalid);
            return;
        }

        var state = GetGameStateForPlayer(players, player, timeControl);
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

    private void HandleMovePiece(GameCommands.MovePiece movePiece, PlayerRoster players)
    {
        if (
            !players.ColorToPlayer.TryGetValue(_gameCore.SideToMove, out var currentPlayer)
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
                WhiteId: players.WhitePlayer.UserId,
                BlackLegalMoves: _gameCore.GetEncodedLegalMovesFor(GameColor.Black),
                BlackId: players.BlackPlayer.UserId,
                SideToMove: _gameCore.SideToMove,
                MoveNumber: _gameCore.MoveNumber
            )
        );
    }

    private GameState GetGameStateForPlayer(
        PlayerRoster players,
        GamePlayer player,
        TimeControlSettings timeControl
    )
    {
        var legalMoves = _gameCore.GetEncodedLegalMovesFor(player.Color);
        var gameState = new GameState(
            PlayerWhite: players.WhitePlayer,
            PlayerBlack: players.BlackPlayer,
            SideToMove: _gameCore.SideToMove,
            Fen: _gameCore.Fen,
            MoveHistory: _gameCore.EncodedMoveHistory,
            LegalMoves: legalMoves,
            TimeControl: timeControl
        );
        return gameState;
    }
}
