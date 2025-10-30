using Chess2.Api.GameLogic.Models;

namespace Chess2.Api.GameLogic.PieceMovementRules;

public class PromotionRule(
    Func<IReadOnlyChessBoard, Move, bool> predicate,
    IReadOnlyCollection<PieceType> promotesTo,
    params IEnumerable<IPieceMovementRule> pieceRules
) : IPieceMovementRule
{
    private readonly IPieceMovementRule[] _pieceRules = [.. pieceRules];
    private readonly Func<IReadOnlyChessBoard, Move, bool> _predicate = predicate;
    private readonly IReadOnlyCollection<PieceType> _promotesTo = promotesTo;

    public IEnumerable<Move> Evaluate(
        IReadOnlyChessBoard board,
        AlgebraicPoint position,
        Piece movingPiece
    )
    {
        foreach (var rule in _pieceRules)
        {
            foreach (var move in rule.Evaluate(board, position, movingPiece))
            {
                if (!_predicate(board, move))
                {
                    yield return move;
                    continue;
                }

                foreach (var pieceType in _promotesTo)
                {
                    yield return move with
                    {
                        PromotesTo = pieceType,
                    };
                }
            }
        }
    }
}
