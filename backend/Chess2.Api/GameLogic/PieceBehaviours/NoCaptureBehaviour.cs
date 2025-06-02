using Chess2.Api.GameLogic.Models;
using Chess2.Api.GameLogic.MovementBehaviours;

namespace Chess2.Api.GameLogic.PieceBehaviours;

public class NoCaptureBehaviour(IMovementBehaviour movementBehaviour) : IPieceBehaviour
{
    private readonly IMovementBehaviour _movementBehaviour = movementBehaviour;

    public IEnumerable<Move> Evaluate(ChessBoard board, Point position, Piece movingPiece)
    {
        foreach (var destination in _movementBehaviour.Evaluate(board, position, movingPiece))
        {
            var isSquareOccupied = board.IsEmpty(destination);
            if (isSquareOccupied)
                yield break;

            yield return new Move(position, destination, movingPiece);
        }
    }
}
