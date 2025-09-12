using Chess2.Api.GameLogic;
using Chess2.Api.GameLogic.Models;
using Chess2.Api.GameSnapshot.Models;
using Chess2.Api.LiveGame.Errors;
using Chess2.Api.LiveGame.Models;
using ErrorOr;

namespace Chess2.Api.LiveGame.Services;

public interface IGameCore
{
    LegalMoveSet GetLegalMovesOf(GameColor? of, GameCoreState state);
    ErrorOr<MoveResult> MakeMove(MoveKey key, GameCoreState state);
    GameColor SideToMove(GameCoreState state);
    string StartGame(GameCoreState state);
}

public readonly record struct MoveResult(
    Move Move,
    MovePath MovePath,
    string San,
    GameEndStatus? EndStatus
);

[GenerateSerializer]
[Alias("Chess2.Api.LiveGame.Services.GameCoreState")]
public class GameCoreState
{
    [Id(0)]
    public ChessBoard Board { get; init; } =
        new(GameConstants.StartingPosition, GameConstants.BoardHeight, GameConstants.BoardWidth);

    [Id(1)]
    public LegalMoveSet LegalMoves { get; set; } = new();

    [Id(2)]
    public AutoDrawState AutoDrawState { get; set; } = new();
}

public class GameCore(
    ILogger<GameCore> logger,
    IFenCalculator fenCalculator,
    ILegalMoveCalculator legalMoveCalculator,
    IMoveEncoder legalMoveEncoder,
    ISanCalculator sanCalculator,
    IDrawEvaulator drawEvaulator,
    IGameResultDescriber resultDescriber
) : IGameCore
{
    private readonly ILogger<GameCore> _logger = logger;
    private readonly IFenCalculator _fenCalculator = fenCalculator;
    private readonly ILegalMoveCalculator _legalMoveCalculator = legalMoveCalculator;
    private readonly IMoveEncoder _moveEncoder = legalMoveEncoder;
    private readonly ISanCalculator _sanCalculator = sanCalculator;
    private readonly IDrawEvaulator _drawEvaulator = drawEvaulator;
    private readonly IGameResultDescriber _resultDescriber = resultDescriber;

    public GameColor SideToMove(GameCoreState state) => state.Board.SideToMove;

    public string StartGame(GameCoreState state)
    {
        var fen = _fenCalculator.CalculateFen(state.Board);
        state.LegalMoves = CalculateAllLegalMoves(state.Board);
        _drawEvaulator.RegisterInitialPosition(fen, state.AutoDrawState);

        return fen;
    }

    public ErrorOr<MoveResult> MakeMove(MoveKey key, GameCoreState state)
    {
        var movingSide = state.Board.SideToMove;
        if (!state.LegalMoves.MovesMap.TryGetValue(key, out var move))
        {
            _logger.LogWarning("Could not find move with key {Key}", key);
            return GameErrors.MoveInvalid;
        }

        state.Board.PlayMove(move);
        var fen = _fenCalculator.CalculateFen(state.Board);

        GameEndStatus? endStatus = null;
        bool isKingCapture = IsKingCapture(move);
        if (isKingCapture)
        {
            endStatus = _resultDescriber.KingCaptured(by: movingSide);
        }
        else if (_drawEvaulator.TryEvaluateDraw(move, fen, state.AutoDrawState, out var drawReason))
        {
            endStatus = drawReason;
        }

        var path = MovePath.FromMove(move, state.Board.Width);
        var san = _sanCalculator.CalculateSan(move, state.LegalMoves.AllMoves, isKingCapture);
        MoveResult moveResult = new(move, path, san, endStatus);

        state.LegalMoves = CalculateAllLegalMoves(state.Board);
        return moveResult;
    }

    public LegalMoveSet GetLegalMovesOf(GameColor? of, GameCoreState state)
    {
        if (of != state.Board.SideToMove)
            return new();
        return state.LegalMoves;
    }

    private LegalMoveSet CalculateAllLegalMoves(ChessBoard board)
    {
        var allMoves = _legalMoveCalculator
            .CalculateAllLegalMoves(board, board.SideToMove)
            .ToList();
        var maxPriority =
            allMoves.Count != 0 ? allMoves.Max(m => m.ForcedPriority) : ForcedMovePriority.None;
        var legalMoves = allMoves.Where(m => m.ForcedPriority == maxPriority).ToList();

        var movePaths = legalMoves.Select(move => MovePath.FromMove(move, board.Width)).ToList();
        var encodedMoves = _moveEncoder.EncodeMoves(movePaths);

        Dictionary<MoveKey, Move> groupedMoves = [];
        foreach (var move in legalMoves)
        {
            MoveKey key = new(From: move.From, To: move.To, PromotesTo: move.PromotesTo);
            if (!groupedMoves.TryAdd(key, move))
            {
                _logger.LogWarning(
                    "Duplicate move found from {From} to {To}. This should never happen",
                    move.From,
                    move.To
                );
                continue;
            }
        }
        return new(
            MovesMap: groupedMoves,
            MovePaths: movePaths,
            EncodedMoves: encodedMoves,
            HasForcedMoves: maxPriority > ForcedMovePriority.None
        );
    }

    private static bool IsKingCapture(Move move) =>
        move.Captures.Any(capture => capture.CapturedPiece.Type is PieceType.King);
}
