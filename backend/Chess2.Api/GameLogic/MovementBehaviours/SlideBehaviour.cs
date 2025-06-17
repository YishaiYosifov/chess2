using Chess2.Api.GameLogic.Models;

namespace Chess2.Api.GameLogic.MovementBehaviours;

public class SlideBehaviour(Offset offset) : IMovementBehaviour
{
    private readonly Offset _offset = offset;

    public IEnumerable<AlgebraicPoint> Evaluate(
        ChessBoard board,
        AlgebraicPoint position,
        Piece movingPiece
    )
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
