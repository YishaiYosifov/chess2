using Chess2.Api.GameSnapshot.Models;
using Chess2.Api.LiveGame.Errors;
using Chess2.Api.LiveGame.Models;
using Chess2.Api.LiveGame.Services;
using Chess2.Api.Users.Models;
using ErrorOr;

namespace Chess2.Api.LiveGame.Actors;

[Alias("Chess2.Api.LiveGame.Actors.IGameGrain")]
public interface IGameGrain : IGrainWithStringKey
{
    [Alias("StartGameAsync")]
    Task StartGameAsync(
        GamePlayer whitePlayer,
        GamePlayer blackPlayer,
        TimeControlSettings timeControl,
        bool isRated
    );

    [Alias("IsGameOngoingAsync")]
    Task<bool> IsGameOngoingAsync();

    [Alias("GetStateAsync")]
    Task<ErrorOr<GameState>> GetStateAsync(UserId forUserId);

    [Alias("GetPlayersAsync")]
    Task<ErrorOr<GamePlayers>> GetPlayersAsync();

    [Alias("EndGameAsync")]
    Task<ErrorOr<Success>> EndGameAsync(UserId byUserId);

    [Alias("RequestDrawAsync")]
    Task<ErrorOr<Success>> RequestDrawAsync(UserId byUserId);

    [Alias("DeclineDrawAsync")]
    Task<ErrorOr<Success>> DeclineDrawAsync(UserId byUserId);

    [Alias("MovePieceAsync")]
    Task<ErrorOr<Success>> MovePieceAsync(UserId byUserId, MoveKey key);
}

public class GameGrain : Grain, IGameGrain
{
    public const string ClockTimerKey = "tickClock";

    private readonly string _token;

    private readonly ILogger<GameGrain> _logger;
    private readonly IGameCore _core;
    private readonly IGameResultDescriber _resultDescriber;
    private readonly IGameNotifier _gameNotifier;
    private readonly IDrawRequestHandler _drawRequestHandler;
    private readonly IGameFinalizer _gameFinalizer;
    private readonly IGameClock _clock;

    private readonly PlayerRoster _players = new();
    private readonly MoveHistoryTracker _historyTracker = new();

    private IGrainTimer? _clockTimer;
    private bool _isPlaying = false;
    private TimeControlSettings _timeControl;
    private GameResultData? _result;
    private bool _isRated;

    public GameGrain(
        ILogger<GameGrain> logger,
        IGameCore core,
        IGameClock clock,
        IGameResultDescriber resultDescriber,
        IGameNotifier gameNotifier,
        IDrawRequestHandler drawRequestHandler,
        IGameFinalizer gameFinalizer
    )
    {
        _token = this.GetPrimaryKeyString();

        _logger = logger;
        _core = core;
        _clock = clock;
        _resultDescriber = resultDescriber;
        _gameNotifier = gameNotifier;
        _drawRequestHandler = drawRequestHandler;
        _gameFinalizer = gameFinalizer;
    }

    public Task StartGameAsync(
        GamePlayer whitePlayer,
        GamePlayer blackPlayer,
        TimeControlSettings timeControl,
        bool isRated
    )
    {
        _players.InitializePlayers(whitePlayer, blackPlayer);
        _core.InitializeGame();
        _clock.Reset(timeControl);

        _timeControl = timeControl;
        _isRated = isRated;

        _isPlaying = true;
        _clockTimer = this.RegisterGrainTimer(
            callback: HandleClockTickAsync,
            dueTime: TimeSpan.Zero,
            period: TimeSpan.FromSeconds(1)
        );
        return Task.CompletedTask;
    }

    public Task<bool> IsGameOngoingAsync() => Task.FromResult(_isPlaying);

    public Task<ErrorOr<GameState>> GetStateAsync(UserId forUserId)
    {
        if (!_isPlaying)
            return Task.FromResult<ErrorOr<GameState>>(GameErrors.GameNotFound);
        if (!_players.TryGetPlayerById(forUserId, out var player))
            return Task.FromResult<ErrorOr<GameState>>(GameErrors.PlayerInvalid);

        var gameState = GetGameStateForPlayer(player);
        return Task.FromResult<ErrorOr<GameState>>(gameState);
    }

    public Task<ErrorOr<GamePlayers>> GetPlayersAsync()
    {
        if (!_isPlaying)
            return Task.FromResult<ErrorOr<GamePlayers>>(GameErrors.GameNotFound);

        return Task.FromResult<ErrorOr<GamePlayers>>(
            new GamePlayers(_players.WhitePlayer, _players.BlackPlayer)
        );
    }

    public async Task<ErrorOr<Success>> EndGameAsync(UserId byUserId)
    {
        if (!_isPlaying)
            return GameErrors.GameNotFound;
        if (!_players.TryGetPlayerById(byUserId, out var player))
            return GameErrors.PlayerInvalid;

        GameEndStatus endStatus;
        var isAbort = _historyTracker.MoveNumber < 2;
        if (isAbort)
            endStatus = _resultDescriber.Aborted(player.Color);
        else
            endStatus = _resultDescriber.Resignation(player.Color);

        _logger.LogInformation(
            "Game {GameToken} ended by user {UserId}. Result: {Result}",
            _token,
            byUserId,
            endStatus.Result
        );
        await FinalizeGameAsync(player, endStatus);
        return Result.Success;
    }

    public async Task<ErrorOr<Success>> RequestDrawAsync(UserId byUserId)
    {
        if (!_isPlaying)
            return GameErrors.GameNotFound;
        if (!_players.TryGetPlayerById(byUserId, out var player))
            return GameErrors.PlayerInvalid;

        if (_drawRequestHandler.HasPendingRequest(player.Color))
        {
            await FinalizeGameAsync(player, _resultDescriber.DrawByAgreement());
            return Result.Success;
        }

        var requestResult = _drawRequestHandler.RequestDraw(player.Color);
        if (requestResult.IsError)
            return requestResult.Errors;

        await _gameNotifier.NotifyDrawStateChangeAsync(_token, _drawRequestHandler.GetState());
        return Result.Success;
    }

    public async Task<ErrorOr<Success>> DeclineDrawAsync(UserId byUserId)
    {
        if (!_isPlaying)
            return GameErrors.GameNotFound;
        if (!_players.TryGetPlayerById(byUserId, out var player))
            return GameErrors.PlayerInvalid;

        if (!_drawRequestHandler.TryDeclineDraw(player.Color))
            return GameErrors.DrawNotRequested;

        await _gameNotifier.NotifyDrawStateChangeAsync(_token, _drawRequestHandler.GetState());
        return Result.Success;
    }

    public async Task<ErrorOr<Success>> MovePieceAsync(UserId byUserId, MoveKey key)
    {
        if (!_isPlaying)
            return GameErrors.GameNotFound;

        var currentPlayer = _players.GetPlayerByColor(_core.SideToMove);
        if (currentPlayer.UserId != byUserId)
        {
            _logger.LogWarning(
                "User {UserId} attmpted to move a piece, but their id doesn't match the current player {PlayingUserId}",
                byUserId,
                currentPlayer?.UserId
            );
            return GameErrors.PlayerInvalid;
        }

        var makeMoveResult = _core.MakeMove(key, currentPlayer.Color);
        if (makeMoveResult.IsError)
            return makeMoveResult.Errors;

        var moveResult = makeMoveResult.Value;
        if (moveResult.EndStatus is not null)
            await FinalizeGameAsync(currentPlayer, moveResult.EndStatus);

        _drawRequestHandler.DecrementCooldown();
        if (_drawRequestHandler.TryDeclineDraw(currentPlayer.Color))
            await _gameNotifier.NotifyDrawStateChangeAsync(_token, _drawRequestHandler.GetState());

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
        return Result.Success;
    }

    private async Task HandleClockTickAsync()
    {
        var timeLeft = _clock.CalculateTimeLeft(_core.SideToMove);
        if (timeLeft > 0)
            return;

        var player = _players.GetPlayerByColor(_core.SideToMove);
        _logger.LogInformation(
            "Game {GameToken} ended by user {UserId} timing out",
            _token,
            player.UserId
        );

        await FinalizeGameAsync(player, _resultDescriber.Timeout(_core.SideToMove));
    }

    private async Task FinalizeGameAsync(GamePlayer endingPlayer, GameEndStatus endStatus)
    {
        _clock.CommitTurn(_core.SideToMove);
        var state = GetGameStateForPlayer(endingPlayer);

        _result = await _gameFinalizer.FinalizeGameAsync(_token, state, endStatus);
        await _gameNotifier.NotifyGameEndedAsync(_token, _result);
        _isPlaying = false;
        _clockTimer?.Dispose();
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
            DrawState: _drawRequestHandler.GetState(),
            ResultData: _result
        );
        return gameState;
    }
}
