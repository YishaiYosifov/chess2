using Chess2.Api.GameLogic.Models;
using Chess2.Api.LiveGame;

namespace Chess2.Api.GameLogic.PieceMovementRules;

public class PromotionRule(
    Func<ChessBoard, Move, bool> predicate,
    params IPieceMovementRule[] pieceRules
) : IPieceMovementRule
{
    private readonly IPieceMovementRule[] _pieceRules = pieceRules;
    private readonly Func<ChessBoard, Move, bool> _predicate = predicate;

    public IEnumerable<Move> Evaluate(ChessBoard board, AlgebraicPoint position, Piece movingPiece)
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

                foreach (var pieceType in GameConstants.PromotablePieces)
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
