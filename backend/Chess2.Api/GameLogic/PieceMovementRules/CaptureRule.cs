using Chess2.Api.GameLogic.Models;
using Chess2.Api.GameLogic.MovementBehaviours;

namespace Chess2.Api.GameLogic.PieceMovementRules;

public class CaptureRule(
    IMovementBehaviour movementBehaviour,
    Func<ChessBoard, Piece, bool>? allowFriendlyFire = null
) : IPieceMovementRule
{
    private readonly IMovementBehaviour _movementBehaviour = movementBehaviour;
    private readonly Func<ChessBoard, Piece, bool>? _allowFriendlyFire = allowFriendlyFire;

    public IEnumerable<Move> Evaluate(ChessBoard board, AlgebraicPoint position, Piece movingPiece)
    {
        foreach (var destination in _movementBehaviour.Evaluate(board, position, movingPiece))
        {
            var occupantPiece = board.PeekPieceAt(destination);

            // skip if friendly fire and friendly fire is not allowed
            if (
                occupantPiece is not null
                && occupantPiece.Color == movingPiece.Color
                && (_allowFriendlyFire is null || !_allowFriendlyFire(board, occupantPiece))
            )
                continue;

            var isCapture = occupantPiece is not null;
            yield return new Move(
                position,
                destination,
                movingPiece,
                capturedSquares: isCapture ? [destination] : null
            );
        }
    }
}
