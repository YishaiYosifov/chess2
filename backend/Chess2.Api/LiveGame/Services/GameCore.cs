using Chess2.Api.GameLogic;
using Chess2.Api.GameLogic.Extensions;
using Chess2.Api.GameLogic.Models;
using Chess2.Api.GameSnapshot.Models;
using Chess2.Api.LiveGame.Errors;
using ErrorOr;
using IReadonlyLegalMoveMap = System.Collections.Generic.IReadOnlyDictionary<
    (
        Chess2.Api.GameLogic.Models.AlgebraicPoint from,
        Chess2.Api.GameLogic.Models.AlgebraicPoint to
    ),
    Chess2.Api.GameLogic.Models.Move
>;
using LegalMoveMap = System.Collections.Generic.Dictionary<
    (
        Chess2.Api.GameLogic.Models.AlgebraicPoint from,
        Chess2.Api.GameLogic.Models.AlgebraicPoint to
    ),
    Chess2.Api.GameLogic.Models.Move
>;

namespace Chess2.Api.LiveGame.Services;

public interface IGameCore
{
    string Fen { get; }
    LegalMoveSet LegalMoves { get; }
    GameColor SideToMove { get; }

    LegalMoveSet GetLegalMovesFor(GameColor forColor);
    void InitializeGame();
    ErrorOr<MoveResult> MakeMove(AlgebraicPoint from, AlgebraicPoint to, GameColor forColor);
}

public record LegalMoveSet(
    IReadonlyLegalMoveMap Moves,
    IReadOnlyCollection<MovePath> MovePaths,
    IReadOnlyCollection<byte> EncodedMoves
)
{
    public IEnumerable<Move> AllMoves => Moves.Values;

    public LegalMoveSet()
        : this(new LegalMoveMap(), [], []) { }
}

public readonly record struct MoveResult(
    Move Move,
    MovePath MovePath,
    string San,
    GameEndStatus? EndStatus
);

public class GameCore(
    ILogger<GameCore> logger,
    IFenCalculator fenCalculator,
    ILegalMoveCalculator legalMoveCalculator,
    IMoveEncoder legalMoveEncoder,
    ISanCalculator sanCalculator,
    IDrawEvaulator drawEvaulator
) : IGameCore
{
    private readonly ChessBoard _board = new(
        GameConstants.StartingPosition,
        GameConstants.BoardHeight,
        GameConstants.BoardWidth
    );

    private readonly ILogger<GameCore> _logger = logger;
    private readonly IFenCalculator _fenCalculator = fenCalculator;
    private readonly ILegalMoveCalculator _legalMoveCalculator = legalMoveCalculator;
    private readonly IMoveEncoder _moveEncoder = legalMoveEncoder;
    private readonly ISanCalculator _sanCalculator = sanCalculator;
    private readonly IDrawEvaulator _drawEvaulator = drawEvaulator;

    public string Fen { get; private set; } = "";
    public LegalMoveSet LegalMoves { get; private set; } = new();
    public GameColor SideToMove { get; private set; } = GameColor.White;

    public void InitializeGame()
    {
        Fen = _fenCalculator.CalculateFen(_board);
        _drawEvaulator.RegisterInitialPosition(Fen);
        CalculateAllLegalMoves(GameColor.White);
    }

    public ErrorOr<MoveResult> MakeMove(AlgebraicPoint from, AlgebraicPoint to, GameColor forColor)
    {
        if (!LegalMoves.Moves.TryGetValue((from, to), out var move))
        {
            _logger.LogWarning("Could not find move from {From} to {To}", from, to);
            return GameErrors.MoveInvalid;
        }

        _board.PlayMove(move);
        var path = MovePath.FromMove(move, _board.Width);
        var san = _sanCalculator.CalculateSan(move, LegalMoves.AllMoves);

        CalculateAllLegalMoves(forColor.Invert());
        Fen = _fenCalculator.CalculateFen(_board);
        SideToMove = SideToMove.Invert();

        GameEndStatus? endStatus = null;
        if (_drawEvaulator.TryEvaluateDraw(move, Fen, out var drawReason))
            endStatus = drawReason;

        return new MoveResult(move, path, san, endStatus);
    }

    public LegalMoveSet GetLegalMovesFor(GameColor forColor)
    {
        if (forColor != SideToMove)
            return new();
        return LegalMoves;
    }

    private void CalculateAllLegalMoves(GameColor forColor)
    {
        var moves = _legalMoveCalculator.CalculateAllLegalMoves(_board, forColor).ToList();
        var movePaths = moves.Select(move => MovePath.FromMove(move, _board.Width)).ToList();
        var encodedMoves = _moveEncoder.EncodeMoves(movePaths);

        LegalMoveMap groupedMoves = [];
        foreach (var move in moves)
        {
            var key = (move.From, move.To);
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
        LegalMoves = new(Moves: groupedMoves, MovePaths: movePaths, EncodedMoves: encodedMoves);
    }
}
