using Chess2.Api.GameLogic.Models;
using Chess2.Api.GameLogic.MovementBehaviours;

namespace Chess2.Api.GameLogic.PieceMovementRules;

public class CaptureRule(
    Func<ChessBoard, Piece, bool>? allowFriendlyFire = null,
    Func<ChessBoard, Piece, bool>? allowNeutralCapture = null,
    params IMovementBehaviour[] movementBehaviours
) : IPieceMovementRule
{
    private readonly IMovementBehaviour[] _movementBehaviour = movementBehaviours;
    private readonly Func<ChessBoard, Piece, bool>? _allowFriendlyFire = allowFriendlyFire;
    private readonly Func<ChessBoard, Piece, bool>? _allowNeutralCapture = allowNeutralCapture;

    public CaptureRule(params IMovementBehaviour[] movementBehaviours)
        : this(null, null, movementBehaviours) { }

    public static CaptureRule WithFriendlyFire(
        Func<ChessBoard, Piece, bool> allowFriendlyFire,
        params IMovementBehaviour[] movementBehaviours
    ) => new(allowFriendlyFire, null, movementBehaviours);

    public static CaptureRule WithNeutralCapture(
        Func<ChessBoard, Piece, bool> allowNeutralCapture,
        params IMovementBehaviour[] movementBehaviours
    ) => new(null, allowNeutralCapture, movementBehaviours);

    public IEnumerable<Move> Evaluate(ChessBoard board, AlgebraicPoint position, Piece movingPiece)
    {
        foreach (var behaviour in _movementBehaviour)
        {
            foreach (var destination in behaviour.Evaluate(board, position, movingPiece))
            {
                var occupantPiece = board.PeekPieceAt(destination);
                if (occupantPiece is not null && !CanCapture(board, movingPiece, occupantPiece))
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

    private bool CanCapture(ChessBoard board, Piece mover, Piece target)
    {
        if (mover.Color is null)
            return _allowNeutralCapture?.Invoke(board, target) ?? true;

        bool isFriendly = mover.Color == target.Color;
        if (!isFriendly)
            return true;

        return _allowFriendlyFire?.Invoke(board, target) ?? false;
    }
}
