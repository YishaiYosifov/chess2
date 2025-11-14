using AnarchyChess.Api.GameLogic.Models;

namespace AnarchyChess.Api.GameLogic.MovementBehaviours;

public class StepBehaviour(Offset offset) : IMovementBehaviour
{
    private readonly Offset _offset = offset;

    public IEnumerable<AlgebraicPoint> Evaluate(
        IReadOnlyChessBoard board,
        AlgebraicPoint position,
        Piece movingPiece
    )
    {
        position += _offset;
        if (!board.IsWithinBoundaries(position))
            yield break;
        yield return position;
    }
}
