using Akka.Actor;
using Akka.Cluster.Sharding;
using Akka.Event;
using Chess2.Api.Game.Errors;
using Chess2.Api.Game.Models;
using Chess2.Api.Game.Services;
using Chess2.Api.GameLogic.Extensions;
using Chess2.Api.Shared.Extensions;

namespace Chess2.Api.Game.Actors;

public class GameActor : ReceiveActor, IWithTimers
{
    private readonly string _token;

    private readonly IGameCore _gameCore;
    private readonly IGameResultDescriber _resultDescriber;
    private readonly IGameNotifier _gameNotifier;

    private readonly GameClock _clock = new();
    private readonly PlayerRoster _players = new();
    private readonly ILoggingAdapter _logger = Context.GetLogger();

    private TimeControlSettings _timeControl;

    public ITimerScheduler Timers { get; set; } = null!;

    public GameActor(
        string token,
        IGameCore game,
        IGameResultDescriber resultDescriber,
        IGameNotifier gameNotifier
    )
    {
        _token = token;
        _gameCore = game;
        _resultDescriber = resultDescriber;
        _gameNotifier = gameNotifier;
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

        ReceiveAny(_ => Sender.Tell(new GameEvents.PieceMoved()));
    }

    private void HandleStartGame(GameCommands.StartGame startGame)
    {
        _players.InitializePlayers(startGame.WhitePlayer, startGame.BlackPlayer);
        _gameCore.InitializeGame();
        _clock.Reset(startGame.TimeControl);

        _timeControl = startGame.TimeControl;

        Timers.StartPeriodicTimer(
            "tickClock",
            new GameCommands.TickClock(),
            TimeSpan.FromSeconds(1)
        );

        Sender.Tell(new GameEvents.GameStartedEvent());
        Become(Playing);
    }

    private void Playing()
    {
        Receive<GameQueries.GetGameStatus>(_ =>
            Sender.Tell(new GameEvents.GameStatusEvent(GameStatus.OnGoing))
        );
        Receive<GameQueries.GetGameState>(HandleGetGameState);

        Receive<GameCommands.TickClock>(_ => HandleClockTick());

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
            Sender.ReplyWithError(GameErrors.PlayerInvalid);
            return;
        }

        var gameState = GetGameStateForPlayer(player);
        Sender.Tell(new GameEvents.GameStateEvent(gameState));
    }

    private void HandleClockTick()
    {
        var timeLeft = _clock.CalculateTimeLeft(_gameCore.SideToMove);
        if (timeLeft > 0)
            return;

        var player = _players.GetPlayerByColor(_gameCore.SideToMove);
        _logger.Info("Game {0} ended by user {1} timing out", _token, player.UserId);

        var reason = _resultDescriber.Timeout(_gameCore.SideToMove);
        var winnerColor = _gameCore.SideToMove.Invert();
        var result = winnerColor.Match(
            whenWhite: GameResult.WhiteWin,
            whenBlack: GameResult.BlackWin
        );
        var state = GetGameStateForPlayer(player);

        Sender.Tell(new GameEvents.GameEnded(result, reason, state));
        Context.Parent.Tell(new Passivate(PoisonPill.Instance));
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
            Sender.ReplyWithError(GameErrors.PlayerInvalid);
            return;
        }

        var state = GetGameStateForPlayer(player);
        GameResult result;
        string reason;

        var isAbort = _gameCore.MoveNumber <= 2;
        if (isAbort)
        {
            result = GameResult.Aborted;
            reason = _resultDescriber.Aborted(player.Color);
        }
        else
        {
            reason = _resultDescriber.Resignation(player.Color);
            var winnerColor = player.Color.Invert();
            result = winnerColor.Match(
                whenWhite: GameResult.WhiteWin,
                whenBlack: GameResult.BlackWin
            );
        }

        _logger.Info("Game {0} ended by user {1}. Result: {2}", _token, endGame.UserId, result);
        Sender.Tell(new GameEvents.GameEnded(result, reason, state));
        //_gameNotifier.NotifyGameEndedAsync(_token, result, reason)
        // TODO: remember to uncomment when done testing
        //Context.Parent.Tell(new Passivate(PoisonPill.Instance));
    }

    private void HandleMovePiece(GameCommands.MovePiece movePiece)
    {
        var currentPlayer = _players.GetPlayerByColor(_gameCore.SideToMove);
        if (currentPlayer.UserId != movePiece.UserId)
        {
            _logger.Warning(
                "User {0} attmpted to move a piece, but their id doesn't match the current player {1}",
                movePiece.UserId,
                currentPlayer?.UserId
            );
            Sender.ReplyWithError(GameErrors.PlayerInvalid);
            return;
        }

        var moveResult = _gameCore.MakeMove(movePiece.From, movePiece.To, currentPlayer.Color);
        if (moveResult.IsError)
        {
            Sender.ReplyWithError(moveResult.Errors);
            return;
        }
        Sender.Tell(new GameEvents.PieceMoved());

        var nextPlayer = _players.GetPlayerByColor(_gameCore.SideToMove);
        RunTask(
            () =>
                _gameNotifier.NotifyMoveMadeAsync(
                    _token,
                    moveResult.Value,
                    _gameCore.SideToMove,
                    _gameCore.MoveNumber,
                    nextPlayer.UserId,
                    _gameCore.GetLegalMovesFor(_gameCore.SideToMove).EncodedMoves
                )
        );
    }

    private GameState GetGameStateForPlayer(GamePlayer player)
    {
        var legalMoves = _gameCore.GetLegalMovesFor(player.Color);
        var gameState = new GameState(
            WhitePlayer: _players.WhitePlayer,
            BlackPlayer: _players.BlackPlayer,
            SideToMove: _gameCore.SideToMove,
            Fen: _gameCore.Fen,
            MoveHistory: _gameCore.EncodedMoveHistory,
            LegalMoves: legalMoves.EncodedMoves,
            TimeControl: _timeControl
        );
        return gameState;
    }
}
