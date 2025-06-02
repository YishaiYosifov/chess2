using Chess2.Api.GameLogic.Models;
using Chess2.Api.GameLogic.MovementBehaviours;

namespace Chess2.Api.GameLogic.PieceBehaviours;

public class CaptureBehaviour(IMovementBehaviour movementBehaviour) : IPieceBehaviour
{
    private readonly IMovementBehaviour _movementBehaviour = movementBehaviour;

    public IEnumerable<Move> Evaluate(ChessBoard board, Point position, Piece movingPiece)
    {
        foreach (var destination in _movementBehaviour.Evaluate(board, position, movingPiece))
        {
            var occupantPiece = board.PeekGetPieceAt(destination);
            var isCapture = occupantPiece is not null && occupantPiece.Color != movingPiece.Color;
            yield return new Move(
                position,
                destination,
                movingPiece,
                CapturedSquares: isCapture ? [destination] : null
            );
        }
    }
}
