using Chess2.Api.GameLogic.Models;

namespace Chess2.Api.GameLogic;

public record PieceRule(
    Point Offset,
    bool CanCapture = false,
    bool CaptureOnly = false,
    bool Slide = false
);

public abstract class MoveEvaluator
{
    protected abstract IEnumerable<PieceRule> GetPieceRules(ChessBoard board, Point position);

    public virtual IEnumerable<Move> CalculateLegalMoves(ChessBoard board, Point position)
    {
        if (!board.TryGetPieceAt(position, out var ourPiece))
            yield break;

        var rules = GetPieceRules(board, position);
        foreach (var rule in rules)
        {
            foreach (var move in EvaluateRule(board, ourPiece, rule, position))
                yield return move;
        }
    }

    private IEnumerable<Move> EvaluateRule(
        ChessBoard board,
        Piece ourPiece,
        PieceRule rule,
        Point position
    )
    {
        var originalPosition = position;
        while (true)
        {
            position += rule.Offset;
            if (!board.IsWithinBoundaries(position))
                yield break;

            var isCapture = !board.IsEmpty(position);
            if (isCapture && rule.CanCapture && !CanCapture(board, ourPiece, position))
                yield break;

            if (isCapture && !rule.CanCapture)
                yield break;

            if (isCapture && !isCapture && rule.CaptureOnly)
                yield break;

            yield return new Move(
                From: originalPosition,
                To: position,
                Piece: ourPiece,
                IsCapture: isCapture
            );

            if (!rule.Slide)
                yield break;
        }
    }

    protected virtual bool CanCapture(ChessBoard board, Piece ourPiece, Point position)
    {
        if (!board.TryGetPieceAt(position, out var attemptToCapture))
            return false;

        return attemptToCapture.Color != ourPiece.Color;
    }
}
