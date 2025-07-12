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
    public const string ClockTimerKey = "tickClock";

    private readonly string _token;
    private readonly IServiceProvider _sp;
    private readonly IGameCore _core;
    private readonly IGameResultDescriber _resultDescriber;
    private readonly IGameNotifier _gameNotifier;
    private readonly IGameClock _clock;

    private readonly PlayerRoster _players = new();
    private readonly MoveHistoryTracker _historyTracker = new();
    private readonly ILoggingAdapter _logger = Context.GetLogger();

    private TimeControlSettings _timeControl;
    private bool _isRated;

    public ITimerScheduler Timers { get; set; } = null!;

    public GameActor(
        string token,
        IServiceProvider sp,
        IGameCore core,
        IGameClock clock,
        IGameResultDescriber resultDescriber,
        IGameNotifier gameNotifier,
        ITimerScheduler? timerScheduler = null
    )
    {
        // for testing
        if (timerScheduler is not null)
            Timers = timerScheduler;
        _token = token;

        _sp = sp;
        _core = core;
        _clock = clock;
        _resultDescriber = resultDescriber;
        _gameNotifier = gameNotifier;
        Become(WaitingForStart);
    }

    private void WaitingForStart()
    {
        Receive<GameQueries.IsGameOngoing>(_ =>
        {
            Sender.Tell(false);
            Context.Parent.Tell(new Passivate(PoisonPill.Instance));
        });
        Receive<GameCommands.StartGame>(HandleStartGame);
        ReceiveAny(_ =>
        {
            Sender.ReplyWithError(GameErrors.GameNotFound);
            Context.Parent.Tell(new Passivate(PoisonPill.Instance));
        });
    }

    private void HandleStartGame(GameCommands.StartGame startGame)
    {
        _players.InitializePlayers(startGame.WhitePlayer, startGame.BlackPlayer);
        _core.InitializeGame();
        _clock.Reset(startGame.TimeControl);

        _timeControl = startGame.TimeControl;
        _isRated = startGame.IsRated;

        Timers.StartPeriodicTimer(
            ClockTimerKey,
            new GameCommands.TickClock(),
            TimeSpan.FromSeconds(1)
        );

        Sender.Tell(new GameEvents.GameStartedEvent());
        Become(Playing);
    }

    private void Playing()
    {
        Receive<GameQueries.IsGameOngoing>(_ => Sender.Tell(true));
        Receive<GameQueries.GetGameState>(HandleGetGameState);

        ReceiveAsync<GameCommands.TickClock>(_ => HandleClockTickAsync());

        ReceiveAsync<GameCommands.EndGame>(HandleEndGameAsync);
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

    private async Task HandleClockTickAsync()
    {
        var timeLeft = _clock.CalculateTimeLeft(_core.SideToMove);
        if (timeLeft > 0)
            return;

        var player = _players.GetPlayerByColor(_core.SideToMove);
        _logger.Info("Game {0} ended by user {1} timing out", _token, player.UserId);

        var reason = _resultDescriber.Timeout(_core.SideToMove);
        var winnerColor = _core.SideToMove.Invert();
        var result = winnerColor.Match(
            whenWhite: GameResult.WhiteWin,
            whenBlack: GameResult.BlackWin
        );

        await FinalizeGameAsync(player, result, reason);
        if (!Sender.IsNobody())
            Sender.Tell(new GameEvents.GameEnded());
    }

    private async Task HandleEndGameAsync(GameCommands.EndGame endGame)
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

        GameResult result;
        string reason;
        var isAbort = _historyTracker.MoveNumber <= 2;
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
        await FinalizeGameAsync(player, result, reason);
        Sender.Tell(new GameEvents.GameEnded());
    }

    private void HandleMovePiece(GameCommands.MovePiece movePiece)
    {
        var currentPlayer = _players.GetPlayerByColor(_core.SideToMove);
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

        var moveResult = _core.MakeMove(movePiece.From, movePiece.To, currentPlayer.Color);
        if (moveResult.IsError)
        {
            Sender.ReplyWithError(moveResult.Errors);
            return;
        }
        var timeLeft = _clock.CommitTurn(currentPlayer.Color);
        var (move, encoded, san) = moveResult.Value;
        var snapshot = _historyTracker.RecordMove(encoded, san, timeLeft);

        var nextPlayer = _players.GetPlayerByColor(_core.SideToMove);
        RunTask(
            () =>
                _gameNotifier.NotifyMoveMadeAsync(
                    _token,
                    snapshot,
                    _core.SideToMove,
                    _historyTracker.MoveNumber,
                    _clock.Value,
                    nextPlayer.UserId,
                    _core.GetLegalMovesFor(_core.SideToMove).EncodedMoves
                )
        );

        Sender.Tell(new GameEvents.PieceMoved());
    }

    private void Finished()
    {
        Receive<GameQueries.IsGameOngoing>(_ =>
        {
            Sender.Tell(false);
            Context.Parent.Tell(new Passivate(PoisonPill.Instance));
        });

        ReceiveAny(_ =>
        {
            Sender.ReplyWithError(GameErrors.GameAlreadyEnded);
            Context.Parent.Tell(new Passivate(PoisonPill.Instance));
        });
    }

    private async Task FinalizeGameAsync(GamePlayer endingPlayer, GameResult result, string reason)
    {
        // TODO: remember to uncomment when done testing
        Context.Parent.Tell(new Passivate(PoisonPill.Instance));

        _clock.CommitTurn(_core.SideToMove);
        var state = GetGameStateForPlayer(endingPlayer);

        await using var scope = _sp.CreateAsyncScope();
        var gameFinalizer = scope.ServiceProvider.GetRequiredService<IGameFinalizer>();
        var archive = await gameFinalizer.FinalizeGameAsync(_token, state, result, reason);
        await _gameNotifier.NotifyGameEndedAsync(
            _token,
            result,
            reason,
            archive.WhitePlayer?.NewRating,
            archive.BlackPlayer?.NewRating
        );
        Become(Finished);
        Timers.Cancel(ClockTimerKey);
    }

    private GameState GetGameStateForPlayer(GamePlayer player)
    {
        var legalMoves = _core.GetLegalMovesFor(player.Color);

        var gameState = new GameState(
            TimeControl: _timeControl,
            IsRated: _isRated,
            WhitePlayer: _players.WhitePlayer,
            BlackPlayer: _players.BlackPlayer,
            Clocks: _clock.Value,
            SideToMove: _core.SideToMove,
            Fen: _core.Fen,
            LegalMoves: legalMoves.EncodedMoves,
            MoveHistory: [.. _historyTracker.MoveHistory]
        );
        return gameState;
    }
}
