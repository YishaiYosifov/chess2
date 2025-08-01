using Chess2.Api.GameLogic.Models;

namespace Chess2.Api.GameLogic.PieceMovementRules;

public class ForcedMoveRule(
    IPieceMovementRule rule,
    ForcedMovePriority priority,
    Func<ChessBoard, Move, bool> predicate
) : IPieceMovementRule
{
    private readonly IPieceMovementRule _rule = rule;
    private readonly ForcedMovePriority _priority = priority;
    private readonly Func<ChessBoard, Move, bool> _predicate = predicate;

    public IEnumerable<Move> Evaluate(ChessBoard board, AlgebraicPoint position, Piece movingPiece)
    {
        foreach (var move in _rule.Evaluate(board, position, movingPiece))
        {
            if (!_predicate(board, move))
            {
                yield return move;
                continue;
            }

            yield return move with
            {
                ForcedPriority = _priority,
            };
        }
    }
}
