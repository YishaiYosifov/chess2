using Chess2.Api.GameLogic.Models;

namespace Chess2.Api.GameLogic.PieceBehaviours;

public interface IPieceBehaviour
{
    public IEnumerable<Move> Evaluate(ChessBoard board, Point position, Piece movingPiece);
}
