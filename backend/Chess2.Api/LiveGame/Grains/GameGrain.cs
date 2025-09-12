using Chess2.Api.GameSnapshot.Models;
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

public class GameGrain : Grain<GameGrainState>, IGameGrain, IGrainBase
{
    public const string ClockTimerKey = "tickClock";

    private readonly string _token;

    private readonly ILogger<GameGrain> _logger;
    private readonly GameSettings _settings;
    private readonly IGameCore _core;
    private readonly IGameResultDescriber _resultDescriber;
    private readonly IGameNotifier _gameNotifier;
    private readonly IGameFinalizer _gameFinalizer;
    private readonly IGameClock _clock;

    private IGrainTimer? _clockTimer;

    public GameGrain(
        ILogger<GameGrain> logger,
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
        _settings = settings.Value.Game;
        _core = core;
        _clock = clock;
        _resultDescriber = resultDescriber;
        _gameNotifier = gameNotifier;
        _gameFinalizer = gameFinalizer;
    }

    public async Task StartGameAsync(GamePlayer whitePlayer, GamePlayer blackPlayer, PoolKey pool)
    {
        _clock.Reset(pool.TimeControl, State.ClockState);

        State.Players = new(whitePlayer, blackPlayer);
        State.InitialFen = _core.StartGame(State.Core);
        State.Pool = pool;

        State.IsGameOngoing = true;
        _clockTimer = this.RegisterGrainTimer(
            callback: HandleClockTickAsync,
            dueTime: TimeSpan.Zero,
            period: TimeSpan.FromSeconds(1)
        );
        await WriteStateAsync();
    }

    public Task<bool> IsGameOngoingAsync() => Task.FromResult(State.IsGameOngoing);

    public Task<ErrorOr<GameState>> GetStateAsync(UserId? forUserId = null)
    {
        if (!PlayingOrDeactivate())
            return Task.FromResult<ErrorOr<GameState>>(GameErrors.GameNotFound);

        GamePlayer? player = null;
        if (forUserId is not null && !State.Players.TryGetPlayerById(forUserId, out player))
            return Task.FromResult<ErrorOr<GameState>>(GameErrors.PlayerInvalid);

        var gameState = GetGameState(player);
        return Task.FromResult<ErrorOr<GameState>>(gameState);
    }

    public Task<ErrorOr<PlayerRoster>> GetPlayersAsync()
    {
        if (!PlayingOrDeactivate())
            return Task.FromResult<ErrorOr<PlayerRoster>>(GameErrors.GameNotFound);

        return Task.FromResult<ErrorOr<PlayerRoster>>(State.Players);
    }

    public async Task<ErrorOr<Success>> RequestGameEndAsync(UserId byUserId)
    {
        if (!PlayingOrDeactivate())
            return GameErrors.GameNotFound;
        if (!State.Players.TryGetPlayerById(byUserId, out var player))
            return GameErrors.PlayerInvalid;

        GameEndStatus endStatus;
        var isAbort = State.MoveSnapshots.Count < 2;
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
        if (!State.Players.TryGetPlayerById(byUserId, out var player))
            return GameErrors.PlayerInvalid;

        if (State.DrawRequest.HasPendingRequest(player.Color))
        {
            await EndGameAsync(_resultDescriber.DrawByAgreement());
            return Result.Success;
        }

        var requestResult = State.DrawRequest.RequestDraw(player.Color);
        if (requestResult.IsError)
            return requestResult.Errors;

        await _gameNotifier.NotifyDrawStateChangeAsync(_token, State.DrawRequest.GetState());
        await WriteStateAsync();
        return Result.Success;
    }

    public async Task<ErrorOr<Success>> DeclineDrawAsync(UserId byUserId)
    {
        if (!PlayingOrDeactivate())
            return GameErrors.GameNotFound;
        if (!State.Players.TryGetPlayerById(byUserId, out var player))
            return GameErrors.PlayerInvalid;

        if (!State.DrawRequest.TryDeclineDraw(player.Color, _settings.DrawCooldown))
            return GameErrors.DrawNotRequested;

        await _gameNotifier.NotifyDrawStateChangeAsync(_token, State.DrawRequest.GetState());
        await WriteStateAsync();
        return Result.Success;
    }

    public async Task<ErrorOr<Success>> MovePieceAsync(UserId byUserId, MoveKey key)
    {
        if (!PlayingOrDeactivate())
            return GameErrors.GameNotFound;

        var currentPlayer = State.Players.GetPlayerByColor(_core.SideToMove(State.Core));
        if (currentPlayer.UserId != byUserId)
        {
            _logger.LogWarning(
                "User {UserId} attmpted to move a piece, but their id doesn't match the current player {PlayingUserId}",
                byUserId,
                currentPlayer?.UserId
            );
            return GameErrors.PlayerInvalid;
        }

        var makeMoveResult = _core.MakeMove(key, State.Core);
        if (makeMoveResult.IsError)
            return makeMoveResult.Errors;
        var moveResult = makeMoveResult.Value;

        State.DrawRequest.DecrementCooldown();
        if (State.DrawRequest.TryDeclineDraw(currentPlayer.Color, _settings.DrawCooldown))
            await _gameNotifier.NotifyDrawStateChangeAsync(_token, State.DrawRequest.GetState());

        var timeLeft = _clock.CommitTurn(currentPlayer.Color, State.ClockState);
        MoveSnapshot moveSnapshot = new(moveResult.MovePath, moveResult.San, timeLeft);
        State.MoveSnapshots.Add(moveSnapshot);

        var nextPlayer = State.Players.GetPlayerByColor(_core.SideToMove(State.Core));
        var legalMoves = _core.GetLegalMovesOf(nextPlayer.Color, State.Core);

        await _gameNotifier.NotifyMoveMadeAsync(
            gameToken: _token,
            move: moveSnapshot,
            moveNumber: State.MoveSnapshots.Count,
            clocks: _clock.ToSnapshot(State.ClockState),
            sideToMove: nextPlayer.Color,
            sideToMoveUserId: nextPlayer.UserId,
            encodedLegalMoves: legalMoves.EncodedMoves,
            hasForcedMoves: legalMoves.HasForcedMoves
        );

        if (moveResult.EndStatus is not null)
            await EndGameAsync(moveResult.EndStatus);
        else
            await WriteStateAsync();

        return Result.Success;
    }

    private async Task HandleClockTickAsync()
    {
        var sideToMove = _core.SideToMove(State.Core);
        var timeLeft = _clock.CalculateTimeLeft(sideToMove, State.ClockState);
        if (timeLeft > 0)
            return;

        var player = State.Players.GetPlayerByColor(sideToMove);
        _logger.LogInformation(
            "Game {GameToken} ended by user {UserId} timing out",
            _token,
            player.UserId
        );

        await EndGameAsync(_resultDescriber.Timeout(sideToMove));
    }

    private bool PlayingOrDeactivate()
    {
        if (!State.IsGameOngoing)
            DeactivateOnIdle();
        return State.IsGameOngoing;
    }

    private async Task EndGameAsync(GameEndStatus endStatus)
    {
        _clock.CommitTurn(_core.SideToMove(State.Core), State.ClockState);
        var state = GetGameState();

        State.Result = await _gameFinalizer.FinalizeGameAsync(_token, state, endStatus);
        await _gameNotifier.NotifyGameEndedAsync(_token, State.Result);

        DeactivateOnIdle();
        await ClearStateAsync();

        _clockTimer?.Dispose();
        _clockTimer = null;
    }

    private GameState GetGameState(GamePlayer? player = null)
    {
        var legalMoves = _core.GetLegalMovesOf(player?.Color, State.Core);

        GameState gameState = new(
            Pool: State.Pool,
            WhitePlayer: State.Players.WhitePlayer,
            BlackPlayer: State.Players.BlackPlayer,
            Clocks: _clock.ToSnapshot(State.ClockState),
            SideToMove: _core.SideToMove(State.Core),
            InitialFen: State.InitialFen,
            MoveOptions: new(
                LegalMoves: legalMoves.MovePaths,
                HasForcedMoves: legalMoves.HasForcedMoves
            ),
            MoveHistory: State.MoveSnapshots,
            DrawState: State.DrawRequest.GetState(),
            ResultData: State.Result
        );
        return gameState;
    }
}
