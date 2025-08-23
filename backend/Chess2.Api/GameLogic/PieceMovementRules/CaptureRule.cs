using Chess2.Api.GameLogic.Models;
using Chess2.Api.GameLogic.MovementBehaviours;

namespace Chess2.Api.GameLogic.PieceMovementRules;

public class CaptureRule(
    Func<ChessBoard, Piece, bool>? allowFriendlyFire = null,
    params IMovementBehaviour[] movementBehaviours
) : IPieceMovementRule
{
    private readonly IMovementBehaviour[] _movementBehaviour = movementBehaviours;
    private readonly Func<ChessBoard, Piece, bool>? _allowFriendlyFire = allowFriendlyFire;

    public CaptureRule(params IMovementBehaviour[] movementBehaviours)
        : this(null, movementBehaviours) { }

    public IEnumerable<Move> Evaluate(ChessBoard board, AlgebraicPoint position, Piece movingPiece)
    {
        foreach (var behaviour in _movementBehaviour)
        {
            foreach (var destination in behaviour.Evaluate(board, position, movingPiece))
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
}
