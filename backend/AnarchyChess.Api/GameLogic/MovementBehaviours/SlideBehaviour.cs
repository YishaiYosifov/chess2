using AnarchyChess.Api.GameLogic.Models;

namespace AnarchyChess.Api.GameLogic.MovementBehaviours;

public class SlideBehaviour(Offset offset, int? max = null) : IMovementBehaviour
{
    private readonly Offset _offset = offset;
    private readonly int? _max = max;

    public IEnumerable<AlgebraicPoint> Evaluate(
        IReadOnlyChessBoard board,
        AlgebraicPoint position,
        Piece movingPiece
    )
    {
        int steps = 0;
        while (true)
        {
            if (_max is not null && steps >= _max)
                yield break;

            position += _offset;
            if (!board.IsWithinBoundaries(position))
                yield break;

            yield return position;

            if (!board.IsEmpty(position))
                yield break;

            steps++;
        }
    }
}
