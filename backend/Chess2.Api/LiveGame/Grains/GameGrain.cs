using Chess2.Api.GameSnapshot.Models;
using Chess2.Api.Infrastructure;
using Chess2.Api.LiveGame.Errors;
using Chess2.Api.LiveGame.Models;
using Chess2.Api.LiveGame.Services;
using Chess2.Api.Matchmaking.Models;
using Chess2.Api.Profile.Models;
using Chess2.Api.Shared.Models;
using ErrorOr;
using Microsoft.Extensions.Options;

namespace Chess2.Api.LiveGame.Grains;

[Alias("Chess2.Api.LiveGame.Grains.IGameGrain")]
public interface IGameGrain : IGrainWithStringKey
{
    [Alias("StartGameAsync")]
    Task StartGameAsync(GamePlayer whitePlayer, GamePlayer blackPlayer, PoolKey pool);

    [Alias("IsGameOngoingAsync")]
    Task<bool> IsGameOngoingAsync();

    [Alias("GetStateAsync")]
    Task<ErrorOr<GameState>> GetStateAsync(UserId? forUserId = null);

    [Alias("GetPlayersAsync")]
    Task<ErrorOr<PlayerRoster>> GetPlayersAsync();

    [Alias("RequestGameEndAsync")]
    Task<ErrorOr<Success>> RequestGameEndAsync(UserId byUserId);

    [Alias("RequestDrawAsync")]
    Task<ErrorOr<Success>> RequestDrawAsync(UserId byUserId);

    [Alias("DeclineDrawAsync")]
    Task<ErrorOr<Success>> DeclineDrawAsync(UserId byUserId);

    [Alias("MovePieceAsync")]
    Task<ErrorOr<Success>> MovePieceAsync(UserId byUserId, MoveKey key);
}

[GenerateSerializer]
[Alias("Chess2.Api.LiveGame.Grains.GameGrainState")]
public class GameGrainState
{
    [Id(0)]
    public bool IsGameOngoing { get; set; }

    [Id(1)]
    public required PlayerRoster Players { get; set; }

    [Id(2)]
    public required PoolKey Pool { get; set; }

    [Id(3)]
    public required string InitialFen { get; set; }

    [Id(4)]
    public List<MoveSnapshot> MoveSnapshots { get; set; } = [];

    [Id(5)]
    public GameCoreState Core { get; set; } = new();

    [Id(6)]
    public DrawRequestState DrawRequest { get; set; } = new();

    [Id(7)]
    public GameClockState ClockState { get; set; } = new();

    [Id(8)]
    public GameResultData? Result;
}

public class GameGrain : Grain, IGameGrain, IGrainBase
{
    public const string ClockTimerKey = "tickClock";
    public const string StateName = "game";

    private readonly string _token;

    private readonly ILogger<GameGrain> _logger;
    private readonly IPersistentState<GameGrainState> _state;

    private readonly GameSettings _settings;
    private readonly IGameCore _core;
    private readonly IGameResultDescriber _resultDescriber;
    private readonly IGameNotifier _gameNotifier;
    private readonly IGameFinalizer _gameFinalizer;
    private readonly IGameClock _clock;

    private IGrainTimer? _clockTimer;

    public GameGrain(
        ILogger<GameGrain> logger,
        [PersistentState(StateName, StorageNames.GameState)] IPersistentState<GameGrainState> state,
        IOptions<AppSettings> settings,
        IGameCore core,
        IGameClock clock,
        IGameResultDescriber resultDescriber,
        IGameNotifier gameNotifier,
        IGameFinalizer gameFinalizer
    )
    {
        _token = this.GetPrimaryKeyString();

        _logger = logger;
        _state = state;
        _settings = settings.Value.Game;
        _core = core;
        _clock = clock;
        _resultDescriber = resultDescriber;
        _gameNotifier = gameNotifier;
        _gameFinalizer = gameFinalizer;
    }

    public async Task StartGameAsync(GamePlayer whitePlayer, GamePlayer blackPlayer, PoolKey pool)
    {
        _clock.Reset(pool.TimeControl, _state.State.ClockState);

        _state.State.Players = new(whitePlayer, blackPlayer);
        _state.State.InitialFen = _core.StartGame(_state.State.Core);
        _state.State.Pool = pool;

        _state.State.IsGameOngoing = true;
        _clockTimer = this.RegisterGrainTimer(
            callback: HandleClockTickAsync,
            dueTime: TimeSpan.Zero,
            period: TimeSpan.FromSeconds(1)
        );
        await _state.WriteStateAsync();
    }

    public Task<bool> IsGameOngoingAsync() => Task.FromResult(_state.State.IsGameOngoing);

    public Task<ErrorOr<GameState>> GetStateAsync(UserId? forUserId = null)
    {
        if (!PlayingOrDeactivate())
            return Task.FromResult<ErrorOr<GameState>>(GameErrors.GameNotFound);

        GamePlayer? player = null;
        if (forUserId is not null && !_state.State.Players.TryGetPlayerById(forUserId, out player))
            return Task.FromResult<ErrorOr<GameState>>(GameErrors.PlayerInvalid);

        var gameState = GetGameState(player);
        return Task.FromResult<ErrorOr<GameState>>(gameState);
    }

    public Task<ErrorOr<PlayerRoster>> GetPlayersAsync()
    {
        if (!PlayingOrDeactivate())
            return Task.FromResult<ErrorOr<PlayerRoster>>(GameErrors.GameNotFound);

        return Task.FromResult<ErrorOr<PlayerRoster>>(_state.State.Players);
    }

    public async Task<ErrorOr<Success>> RequestGameEndAsync(UserId byUserId)
    {
        if (!PlayingOrDeactivate())
            return GameErrors.GameNotFound;
        if (!_state.State.Players.TryGetPlayerById(byUserId, out var player))
            return GameErrors.PlayerInvalid;

        GameEndStatus endStatus;
        var isAbort = _state.State.MoveSnapshots.Count < 2;
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
        await EndGameAsync(endStatus);

        return Result.Success;
    }

    public async Task<ErrorOr<Success>> RequestDrawAsync(UserId byUserId)
    {
        if (!PlayingOrDeactivate())
            return GameErrors.GameNotFound;
        if (!_state.State.Players.TryGetPlayerById(byUserId, out var player))
            return GameErrors.PlayerInvalid;

        if (_state.State.DrawRequest.HasPendingRequest(player.Color))
        {
            await EndGameAsync(_resultDescriber.DrawByAgreement());
            return Result.Success;
        }

        var requestResult = _state.State.DrawRequest.RequestDraw(player.Color);
        if (requestResult.IsError)
            return requestResult.Errors;

        await _gameNotifier.NotifyDrawStateChangeAsync(_token, _state.State.DrawRequest.GetState());
        await _state.WriteStateAsync();
        return Result.Success;
    }

    public async Task<ErrorOr<Success>> DeclineDrawAsync(UserId byUserId)
    {
        if (!PlayingOrDeactivate())
            return GameErrors.GameNotFound;
        if (!_state.State.Players.TryGetPlayerById(byUserId, out var player))
            return GameErrors.PlayerInvalid;

        if (!_state.State.DrawRequest.TryDeclineDraw(player.Color, _settings.DrawCooldown))
            return GameErrors.DrawNotRequested;

        await _gameNotifier.NotifyDrawStateChangeAsync(_token, _state.State.DrawRequest.GetState());
        await _state.WriteStateAsync();
        return Result.Success;
    }

    public async Task<ErrorOr<Success>> MovePieceAsync(UserId byUserId, MoveKey key)
    {
        if (!PlayingOrDeactivate())
            return GameErrors.GameNotFound;

        var currentPlayer = _state.State.Players.GetPlayerByColor(
            _core.SideToMove(_state.State.Core)
        );
        if (currentPlayer.UserId != byUserId)
        {
            _logger.LogWarning(
                "User {UserId} attmpted to move a piece, but their id doesn't match the current player {PlayingUserId}",
                byUserId,
                currentPlayer?.UserId
            );
            return GameErrors.PlayerInvalid;
        }

        var makeMoveResult = _core.MakeMove(key, _state.State.Core);
        if (makeMoveResult.IsError)
            return makeMoveResult.Errors;
        var moveResult = makeMoveResult.Value;

        _state.State.DrawRequest.DecrementCooldown();
        if (_state.State.DrawRequest.TryDeclineDraw(currentPlayer.Color, _settings.DrawCooldown))
            await _gameNotifier.NotifyDrawStateChangeAsync(
                _token,
                _state.State.DrawRequest.GetState()
            );

        var timeLeft = _clock.CommitTurn(currentPlayer.Color, _state.State.ClockState);
        MoveSnapshot moveSnapshot = new(moveResult.MovePath, moveResult.San, timeLeft);
        _state.State.MoveSnapshots.Add(moveSnapshot);

        var nextPlayer = _state.State.Players.GetPlayerByColor(_core.SideToMove(_state.State.Core));
        var legalMoves = _core.GetLegalMovesOf(nextPlayer.Color, _state.State.Core);

        await _gameNotifier.NotifyMoveMadeAsync(
            gameToken: _token,
            move: moveSnapshot,
            moveNumber: _state.State.MoveSnapshots.Count,
            clocks: _clock.ToSnapshot(_state.State.ClockState),
            sideToMove: nextPlayer.Color,
            sideToMoveUserId: nextPlayer.UserId,
            encodedLegalMoves: legalMoves.EncodedMoves,
            hasForcedMoves: legalMoves.HasForcedMoves
        );

        if (moveResult.EndStatus is not null)
            await EndGameAsync(moveResult.EndStatus);
        else
            await _state.WriteStateAsync();

        return Result.Success;
    }

    private async Task HandleClockTickAsync()
    {
        var sideToMove = _core.SideToMove(_state.State.Core);
        var timeLeft = _clock.CalculateTimeLeft(sideToMove, _state.State.ClockState);
        if (timeLeft > 0)
            return;

        var player = _state.State.Players.GetPlayerByColor(sideToMove);
        _logger.LogInformation(
            "Game {GameToken} ended by user {UserId} timing out",
            _token,
            player.UserId
        );

        await EndGameAsync(_resultDescriber.Timeout(sideToMove));
    }

    private bool PlayingOrDeactivate()
    {
        if (!_state.State.IsGameOngoing)
            DeactivateOnIdle();
        return _state.State.IsGameOngoing;
    }

    private async Task EndGameAsync(GameEndStatus endStatus)
    {
        _clock.CommitTurn(_core.SideToMove(_state.State.Core), _state.State.ClockState);
        var state = GetGameState();

        _state.State.Result = await _gameFinalizer.FinalizeGameAsync(
            _token,
            state,
            endStatus,
            _state.State.Core.Board.Moves
        );
        await _gameNotifier.NotifyGameEndedAsync(_token, _state.State.Result);

        DeactivateOnIdle();
        await _state.ClearStateAsync();

        _clockTimer?.Dispose();
        _clockTimer = null;
    }

    private GameState GetGameState(GamePlayer? player = null)
    {
        var legalMoves = _core.GetLegalMovesOf(player?.Color, _state.State.Core);

        GameState gameState = new(
            Pool: _state.State.Pool,
            WhitePlayer: _state.State.Players.WhitePlayer,
            BlackPlayer: _state.State.Players.BlackPlayer,
            Clocks: _clock.ToSnapshot(_state.State.ClockState),
            SideToMove: _core.SideToMove(_state.State.Core),
            InitialFen: _state.State.InitialFen,
            MoveOptions: new(
                LegalMoves: legalMoves.MovePaths,
                HasForcedMoves: legalMoves.HasForcedMoves
            ),
            MoveHistory: _state.State.MoveSnapshots,
            DrawState: _state.State.DrawRequest.GetState(),
            ResultData: _state.State.Result
        );
        return gameState;
    }
}
