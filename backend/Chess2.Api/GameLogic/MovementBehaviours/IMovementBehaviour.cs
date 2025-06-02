using Chess2.Api.GameLogic.Models;

namespace Chess2.Api.GameLogic.MovementBehaviours;

public interface IMovementBehaviour
{
    public IEnumerable<Point> Evaluate(ChessBoard board, Point position, Piece movingPiece);
}
