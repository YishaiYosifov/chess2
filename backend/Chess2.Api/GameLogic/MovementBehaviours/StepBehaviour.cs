using Chess2.Api.GameLogic.Models;

namespace Chess2.Api.GameLogic.MovementBehaviours;

public class StepBehaviour(Point offset) : IMovementBehaviour
{
    private readonly Point _offset = offset;

    public IEnumerable<Point> Evaluate(ChessBoard board, Point position, Piece movingPiece)
    {
        position += _offset;
        if (!board.IsWithinBoundaries(position))
            yield break;
        yield return position;
    }
}
