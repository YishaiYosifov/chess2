using Chess2.Api.GameLogic.Models;

namespace Chess2.Api.GameLogic.PieceMovementRules;

public class ForcedMoveRule(
    ForcedMovePriority priority,
    Func<ChessBoard, Move, bool> predicate,
    params IPieceMovementRule[] rules
) : IPieceMovementRule
{
    private readonly IPieceMovementRule[] _rules = rules;
    private readonly ForcedMovePriority _priority = priority;
    private readonly Func<ChessBoard, Move, bool> _predicate = predicate;

    public IEnumerable<Move> Evaluate(ChessBoard board, AlgebraicPoint position, Piece movingPiece)
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
