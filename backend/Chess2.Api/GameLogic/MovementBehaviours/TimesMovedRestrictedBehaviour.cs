using Chess2.Api.GameLogic.Models;

namespace Chess2.Api.GameLogic.MovementBehaviours;

public class TimesMovedRestrictedBehaviour(IMovementBehaviour movementBehaviour, int maxTimesMoved)
    : IMovementBehaviour
{
    private readonly IMovementBehaviour _movementBehaviour = movementBehaviour;
    private readonly int _maxTimesMoved = maxTimesMoved;

    public IEnumerable<AlgebraicPoint> Evaluate(
        ChessBoard board,
        AlgebraicPoint position,
        Piece movingPiece
    )
    {
        return movingPiece.TimesMoved > _maxTimesMoved
            ? []
            : _movementBehaviour.Evaluate(board, position, movingPiece);
    }
}
