using System.Diagnostics.CodeAnalysis;
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
    Task<bool> DoesGameExistAsync();

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
[Alias("Chess2.Api.LiveGame.Grains.GameData")]
public class GameData()
{
    [Id(1)]
    public required PlayerRoster Players { get; set; }

    [Id(2)]
    public required PoolKey Pool { get; set; }

    [Id(3)]
    public required string InitialFen { get; set; }

    [Id(4)]
    public List<MoveSnapshot> MoveSnapshots { get; set; } = [];

    [Id(5)]
    public required GameCoreState Core { get; set; }

    [Id(6)]
    public required DrawRequestState DrawRequest { get; set; }

    [Id(7)]
    public required GameClockState ClockState { get; set; }

    [Id(8)]
    public GameResultData? Result;
}

[GenerateSerializer]
[Alias("Chess2.Api.LiveGame.Grains.GameGrainState")]
public class GameGrainState
{
    [Id(0)]
    public GameData? CurrentGame { get; set; }
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
        PlayerRoster players = new(whitePlayer, blackPlayer);
        GameCoreState core = new();
        DrawRequestState drawRequest = new();
        GameClockState clockState = new();

        _state.State.CurrentGame = new()
        {
            Players = players,
            Pool = pool,
            InitialFen = _core.StartGame(core),
            Core = core,
            DrawRequest = drawRequest,
            ClockState = clockState,
        };
        _clock.Reset(pool.TimeControl, clockState);

        _clockTimer = this.RegisterGrainTimer(
            callback: HandleClockTickAsync,
            dueTime: TimeSpan.Zero,
            period: TimeSpan.FromSeconds(1)
        );
        await _state.WriteStateAsync();
    }

    public Task<bool> DoesGameExistAsync() => Task.FromResult(_state.State.CurrentGame is not null);

    public Task<ErrorOr<GameState>> GetStateAsync(UserId? forUserId = null)
    {
        if (!TryGetCurrentGame(out var game))
            return Task.FromResult<ErrorOr<GameState>>(GameErrors.GameNotFound);

        GamePlayer? player = null;
        if (forUserId is not null && !game.Players.TryGetPlayerById(forUserId, out player))
            return Task.FromResult<ErrorOr<GameState>>(GameErrors.PlayerInvalid);

        var gameState = GetGameState(game, player);
        return Task.FromResult<ErrorOr<GameState>>(gameState);
    }

    public Task<ErrorOr<PlayerRoster>> GetPlayersAsync() =>
        Task.FromResult<ErrorOr<PlayerRoster>>(
            TryGetCurrentGame(out var game) ? game.Players : GameErrors.GameNotFound
        );

    public async Task<ErrorOr<Success>> RequestGameEndAsync(UserId byUserId)
    {
        if (!TryGetCurrentGame(out var game))
            return GameErrors.GameNotFound;
        if (!game.Players.TryGetPlayerById(byUserId, out var player))
            return GameErrors.PlayerInvalid;

        GameEndStatus endStatus;
        var isAbort = game.MoveSnapshots.Count < 2;
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
        await EndGameAsync(endStatus, game);

        return Result.Success;
    }

    public async Task<ErrorOr<Success>> RequestDrawAsync(UserId byUserId)
    {
        if (!TryGetCurrentGame(out var game))
            return GameErrors.GameNotFound;
        if (!game.Players.TryGetPlayerById(byUserId, out var player))
            return GameErrors.PlayerInvalid;

        if (game.DrawRequest.HasPendingRequest(player.Color))
        {
            await EndGameAsync(_resultDescriber.DrawByAgreement(), game);
            return Result.Success;
        }

        var requestResult = game.DrawRequest.RequestDraw(player.Color);
        if (requestResult.IsError)
            return requestResult.Errors;

        await _gameNotifier.NotifyDrawStateChangeAsync(_token, game.DrawRequest.GetState());
        await _state.WriteStateAsync();
        return Result.Success;
    }

    public async Task<ErrorOr<Success>> DeclineDrawAsync(UserId byUserId)
    {
        if (!TryGetCurrentGame(out var game))
            return GameErrors.GameNotFound;
        if (!game.Players.TryGetPlayerById(byUserId, out var player))
            return GameErrors.PlayerInvalid;

        if (!game.DrawRequest.TryDeclineDraw(player.Color, _settings.DrawCooldown))
            return GameErrors.DrawNotRequested;

        await _gameNotifier.NotifyDrawStateChangeAsync(_token, game.DrawRequest.GetState());
        await _state.WriteStateAsync();
        return Result.Success;
    }

    public async Task<ErrorOr<Success>> MovePieceAsync(UserId byUserId, MoveKey key)
    {
        if (!TryGetCurrentGame(out var game))
            return GameErrors.GameNotFound;

        var currentPlayer = game.Players.GetPlayerByColor(_core.SideToMove(game.Core));
        if (currentPlayer.UserId != byUserId)
        {
            _logger.LogWarning(
                "User {UserId} attmpted to move a piece, but their id doesn't match the current player {PlayingUserId}",
                byUserId,
                currentPlayer?.UserId
            );
            return GameErrors.PlayerInvalid;
        }

        var makeMoveResult = _core.MakeMove(key, game.Core);
        if (makeMoveResult.IsError)
            return makeMoveResult.Errors;
        var moveResult = makeMoveResult.Value;

        game.DrawRequest.DecrementCooldown();
        if (game.DrawRequest.TryDeclineDraw(currentPlayer.Color, _settings.DrawCooldown))
            await _gameNotifier.NotifyDrawStateChangeAsync(_token, game.DrawRequest.GetState());

        var timeLeft = _clock.CommitTurn(currentPlayer.Color, game.ClockState);
        MoveSnapshot moveSnapshot = new(moveResult.MovePath, moveResult.San, timeLeft);
        game.MoveSnapshots.Add(moveSnapshot);

        var nextPlayer = game.Players.GetPlayerByColor(_core.SideToMove(game.Core));
        var legalMoves = _core.GetLegalMovesOf(nextPlayer.Color, game.Core);

        await _gameNotifier.NotifyMoveMadeAsync(
            gameToken: _token,
            move: moveSnapshot,
            moveNumber: game.MoveSnapshots.Count,
            clocks: _clock.ToSnapshot(game.ClockState),
            sideToMove: nextPlayer.Color,
            sideToMoveUserId: nextPlayer.UserId,
            encodedLegalMoves: legalMoves.EncodedMoves,
            hasForcedMoves: legalMoves.HasForcedMoves
        );

        if (moveResult.EndStatus is not null)
            await EndGameAsync(moveResult.EndStatus, game);
        else
            await _state.WriteStateAsync();

        return Result.Success;
    }

    private async Task HandleClockTickAsync()
    {
        if (!TryGetCurrentGame(out var game))
            return;

        var sideToMove = _core.SideToMove(game.Core);
        var timeLeft = _clock.CalculateTimeLeft(sideToMove, game.ClockState);
        if (timeLeft > 0)
            return;

        var player = game.Players.GetPlayerByColor(sideToMove);
        _logger.LogInformation(
            "Game {GameToken} ended by user {UserId} timing out",
            _token,
            player.UserId
        );

        await EndGameAsync(_resultDescriber.Timeout(sideToMove), game);
    }

    private async Task EndGameAsync(GameEndStatus endStatus, GameData game)
    {
        _clock.CommitTurn(_core.SideToMove(game.Core), game.ClockState);
        var state = GetGameState(game: game);

        game.Result = await _gameFinalizer.FinalizeGameAsync(
            _token,
            state,
            endStatus,
            game.Core.Board.Moves
        );
        await _gameNotifier.NotifyGameEndedAsync(_token, game.Result);

        _clockTimer?.Dispose();
        _clockTimer = null;
    }

    private GameState GetGameState(GameData game, GamePlayer? player = null)
    {
        // there are only legal moves if the game is not over
        MoveOptions moveOptions;
        if (game.Result is null)
        {
            var legalMoves = _core.GetLegalMovesOf(player?.Color, game.Core);
            moveOptions = new(
                LegalMoves: legalMoves.MovePaths,
                HasForcedMoves: legalMoves.HasForcedMoves
            );
        }
        else
        {
            moveOptions = new();
        }

        GameState gameState = new(
            Pool: game.Pool,
            WhitePlayer: game.Players.WhitePlayer,
            BlackPlayer: game.Players.BlackPlayer,
            Clocks: _clock.ToSnapshot(game.ClockState),
            SideToMove: _core.SideToMove(game.Core),
            InitialFen: game.InitialFen,
            MoveOptions: moveOptions,
            MoveHistory: game.MoveSnapshots,
            DrawState: game.DrawRequest.GetState(),
            ResultData: game.Result
        );
        return gameState;
    }

    private bool TryGetCurrentGame([NotNullWhen(true)] out GameData? state)
    {
        state = _state.State.CurrentGame;
        return state is not null;
    }
}
