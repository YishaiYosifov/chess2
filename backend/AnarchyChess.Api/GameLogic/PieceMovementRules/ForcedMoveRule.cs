using AnarchyChess.Api.GameLogic.Models;

namespace AnarchyChess.Api.GameLogic.PieceMovementRules;

public class ForcedMoveRule(
    ForcedMovePriority priority,
    Func<IReadOnlyChessBoard, Move, bool> predicate,
    params IPieceMovementRule[] rules
) : IPieceMovementRule
{
    private readonly IPieceMovementRule[] _rules = rules;
    private readonly ForcedMovePriority _priority = priority;
    private readonly Func<IReadOnlyChessBoard, Move, bool> _predicate = predicate;

    public IEnumerable<Move> Evaluate(
        IReadOnlyChessBoard board,
        AlgebraicPoint position,
        Piece movingPiece
    )
    {
        foreach (var rule in _rules)
        {
            foreach (var move in rule.Evaluate(board, position, movingPiece))
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
}
