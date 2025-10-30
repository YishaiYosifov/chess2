using Chess2.Api.GameLogic.Models;

namespace Chess2.Api.GameLogic.PieceMovementRules;

public class MoveToSelfRule : IPieceMovementRule
{
    public IEnumerable<Move> Evaluate(
        IReadOnlyChessBoard board,
        AlgebraicPoint position,
        Piece movingPiece
    )
    {
        yield return new Move(from: position, to: position, piece: movingPiece);
    }
}
