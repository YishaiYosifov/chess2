using Chess2.Api.GameLogic.Models;
using Chess2.Api.GameLogic.MovementBehaviours;

namespace Chess2.Api.GameLogic.PieceMovementRules;

public class CaptureOnlyRule(IMovementBehaviour movementBehaviour) : IPieceMovementRule
{
    private readonly IMovementBehaviour _movementBehaviour = movementBehaviour;

    public IEnumerable<Move> Evaluate(ChessBoard board, AlgebraicPoint position, Piece movingPiece)
    {
        foreach (var destination in _movementBehaviour.Evaluate(board, position, movingPiece))
        {
            var occupantPiece = board.PeekPieceAt(destination);
            if (occupantPiece is null || occupantPiece.Color == movingPiece.Color)
                continue;

            yield return new Move(
                position,
                destination,
                movingPiece,
                capturedSquares: [destination]
            );
        }
    }
}
