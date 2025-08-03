using System.Diagnostics.CodeAnalysis;
using Akka.Actor;
using Akka.Cluster.Sharding;
using Akka.Event;
using Chess2.Api.GameSnapshot.Models;
using Chess2.Api.LiveGame.Errors;
using Chess2.Api.LiveGame.Models;
using Chess2.Api.LiveGame.Services;
using Chess2.Api.Shared.Extensions;

namespace Chess2.Api.LiveGame.Actors;

public class GameActor : ReceiveActor, IWithTimers
{
    public const string ClockTimerKey = "tickClock";

    private readonly string _token;
    private readonly IServiceProvider _sp;
    private readonly IGameCore _core;
    private readonly IGameResultDescriber _resultDescriber;
    private readonly IGameNotifier _gameNotifier;
    private readonly IDrawRequestHandler _drawRequestHandler;
    private readonly IGameClock _clock;

    private readonly PlayerRoster _players = new();
    private readonly MoveHistoryTracker _historyTracker = new();
    private readonly ILoggingAdapter _logger = Context.GetLogger();

    private TimeControlSettings _timeControl;
    private bool _isRated;
    private GameResultData? _result;

    public ITimerScheduler Timers { get; set; } = null!;

    public GameActor(
        string token,
        IServiceProvider sp,
        IGameCore core,
        IGameClock clock,
        IGameResultDescriber resultDescriber,
        IGameNotifier gameNotifier,
        IDrawRequestHandler drawRequestHandler,
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
        _drawRequestHandler = drawRequestHandler;
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

        Sender.Tell(new GameResponses.GameStarted());
        Become(Playing);
    }

    private void Playing()
    {
        Receive<GameQueries.IsGameOngoing>(_ => Sender.Tell(true));
        Receive<GameQueries.GetGameState>(HandleGetGameState);
        Receive<GameQueries.GetGamePlayers>(_ =>
            Sender.Tell(new GameResponses.GamePlayers(_players.WhitePlayer, _players.BlackPlayer))
        );

        ReceiveAsync<GameCommands.TickClock>(_ => HandleClockTickAsync());

        ReceiveAsync<GameCommands.EndGame>(HandleEndGameAsync);
        ReceiveAsync<GameCommands.RequestDraw>(HandleDrawRequestAsync);
        ReceiveAsync<GameCommands.DeclineDraw>(HandleDrawDeclineAsync);
        ReceiveAsync<GameCommands.MovePiece>(HandleMovePieceAsync);
    }

    private void HandleGetGameState(GameQueries.GetGameState getGameState)
    {
        if (!TryGetPlayer(getGameState.ForUserId, out var player))
            return;

        var gameState = GetGameStateForPlayer(player);
        Sender.Tell(new GameResponses.GameStateResponse(gameState));
    }

    private async Task HandleClockTickAsync()
    {
        var timeLeft = _clock.CalculateTimeLeft(_core.SideToMove);
        if (timeLeft > 0)
            return;

        var player = _players.GetPlayerByColor(_core.SideToMove);
        _logger.Info("Game {0} ended by user {1} timing out", _token, player.UserId);

        await FinalizeGameAsync(player, _resultDescriber.Timeout(_core.SideToMove));
        if (!Sender.IsNobody())
            Sender.Tell(new GameResponses.GameEnded());
    }

    private async Task HandleDrawRequestAsync(GameCommands.RequestDraw requestDraw)
    {
        if (!TryGetPlayer(requestDraw.UserId, out var player))
            return;

        if (_drawRequestHandler.HasPendingRequest(player.Color))
        {
            await FinalizeGameAsync(player, _resultDescriber.DrawByAgreement());
            Sender.Tell(new GameResponses.DrawRequested());
            return;
        }

        var requestResult = _drawRequestHandler.RequestDraw(player.Color);
        if (requestResult.IsError)
        {
            Sender.ReplyWithError(requestResult.Errors);
            return;
        }
        await _gameNotifier.NotifyDrawRequestAsync(_token);
        Sender.Tell(new GameResponses.DrawRequested());
    }

    private async Task HandleDrawDeclineAsync(GameCommands.DeclineDraw declineDraw)
    {
        if (!TryGetPlayer(declineDraw.UserId, out var _))
            return;

        if (!_drawRequestHandler.TryDeclineDraw())
        {
            Sender.ReplyWithError(GameErrors.DrawNotRequested);
            return;
        }

        await _gameNotifier.NotifyDrawDeclinedAsync(_token);
        Sender.Tell(new GameResponses.DrawDeclined());
    }

    private async Task HandleEndGameAsync(GameCommands.EndGame endGame)
    {
        if (!TryGetPlayer(endGame.UserId, out var player))
            return;

        GameEndStatus endStatus;
        var isAbort = _historyTracker.MoveNumber < 2;
        if (isAbort)
            endStatus = _resultDescriber.Aborted(player.Color);
        else
            endStatus = _resultDescriber.Resignation(player.Color);

        _logger.Info(
            "Game {0} ended by user {1}. Result: {2}",
            _token,
            endGame.UserId,
            endStatus.Result
        );
        await FinalizeGameAsync(player, endStatus);
        Sender.Tell(new GameResponses.GameEnded());
    }

    private async Task HandleMovePieceAsync(GameCommands.MovePiece movePiece)
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

        var makeMoveResult = _core.MakeMove(movePiece.Key, currentPlayer.Color);
        if (makeMoveResult.IsError)
        {
            Sender.ReplyWithError(makeMoveResult.Errors);
            return;
        }
        var moveResult = makeMoveResult.Value;

        if (moveResult.EndStatus is not null)
            await FinalizeGameAsync(currentPlayer, moveResult.EndStatus);

        _drawRequestHandler.DecrementCooldown();
        if (_drawRequestHandler.TryDeclineDraw())
            await _gameNotifier.NotifyDrawDeclinedAsync(_token);

        var timeLeft = _clock.CommitTurn(currentPlayer.Color);
        var moveSnapshot = _historyTracker.RecordMove(
            moveResult.MovePath,
            moveResult.San,
            timeLeft
        );

        var legalMoves = _core.GetLegalMovesFor(_core.SideToMove);
        var nextPlayer = _players.GetPlayerByColor(_core.SideToMove);
        await _gameNotifier.NotifyMoveMadeAsync(
            gameToken: _token,
            move: moveSnapshot,
            moveNumber: _historyTracker.MoveNumber,
            clocks: _clock.Value,
            sideToMove: _core.SideToMove,
            sideToMoveUserId: nextPlayer.UserId,
            encodedLegalMoves: legalMoves.EncodedMoves,
            hasForcedMoves: legalMoves.HasForcedMoves
        );
        Sender.Tell(new GameResponses.PieceMoved());
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

    private async Task FinalizeGameAsync(GamePlayer endingPlayer, GameEndStatus endStatus)
    {
        Context.Parent.Tell(new Passivate(PoisonPill.Instance));

        _clock.CommitTurn(_core.SideToMove);
        var state = GetGameStateForPlayer(endingPlayer);

        await using var scope = _sp.CreateAsyncScope();
        var gameFinalizer = scope.ServiceProvider.GetRequiredService<IGameFinalizer>();

        _result = await gameFinalizer.FinalizeGameAsync(_token, state, endStatus);
        await _gameNotifier.NotifyGameEndedAsync(_token, _result);
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
            InitialFen: _core.InitialFen,
            MoveOptions: new(
                LegalMoves: legalMoves.MovePaths,
                HasForcedMoves: legalMoves.HasForcedMoves
            ),
            MoveHistory: _historyTracker.MoveHistory,
            DrawState: _drawRequestHandler.GetDrawState(),
            ResultData: _result
        );
        return gameState;
    }

    private bool TryGetPlayer(string userId, [NotNullWhen(true)] out GamePlayer? player)
    {
        if (!_players.TryGetPlayerById(userId, out player))
        {
            _logger.Warning("Could not find player {0} in game {1}", userId, _token);
            Sender.ReplyWithError(GameErrors.PlayerInvalid);
            return false;
        }
        return true;
    }
}
