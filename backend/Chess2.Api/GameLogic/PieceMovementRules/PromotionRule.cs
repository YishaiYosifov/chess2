using Chess2.Api.GameLogic.Models;

namespace Chess2.Api.GameLogic.PieceMovementRules;

public class PromotionRule(
    Func<ChessBoard, Move, bool> predicate,
    params IPieceMovementRule[] pieceRules
) : IPieceMovementRule
{
    private readonly IPieceMovementRule[] _pieceRules = pieceRules;
    private readonly Func<ChessBoard, Move, bool> _predicate = predicate;

    private static readonly HashSet<PieceType> _cantPromoteTo =
    [
        PieceType.King,
        PieceType.Pawn,
        PieceType.UnderagePawn,
    ];
    private static readonly List<PieceType> _promotesTo =
    [
        .. Enum.GetValues<PieceType>().Where(p => !_cantPromoteTo.Contains(p)),
    ];

    public IEnumerable<Move> Evaluate(ChessBoard board, AlgebraicPoint position, Piece movingPiece)
    {
        foreach (var rule in _pieceRules)
        {
            foreach (var move in rule.Evaluate(board, position, movingPiece))
            {
                yield return move;
                if (!_predicate(board, move))
                    continue;

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
