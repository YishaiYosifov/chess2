using Chess2.Api.GameLogic.Models;

namespace Chess2.Api.GameLogic.MovementBehaviours;

public class SlideBehaviour(Point offset) : IMovementBehaviour
{
    private readonly Point _offset = offset;

    public IEnumerable<Point> Evaluate(ChessBoard board, Point position, Piece movingPiece)
    {
        while (true)
        {
            position += _offset;
            if (!board.IsWithinBoundaries(position))
                yield break;

            yield return position;

            if (!board.IsEmpty(position))
                yield break;
        }
    }
}
