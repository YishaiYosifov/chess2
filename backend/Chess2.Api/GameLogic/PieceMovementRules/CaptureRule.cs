using Chess2.Api.GameLogic.Models;
using Chess2.Api.GameLogic.MovementBehaviours;

namespace Chess2.Api.GameLogic.PieceMovementRules;

public class CaptureRule(
    Func<IReadOnlyChessBoard, Piece, bool>? allowFriendlyFireWhen = null,
    Func<IReadOnlyChessBoard, Piece, bool>? allowNeutralCaptureWhen = null,
    params IMovementBehaviour[] movementBehaviours
) : IPieceMovementRule
{
    private readonly IMovementBehaviour[] _movementBehaviour = movementBehaviours;
    private readonly Func<IReadOnlyChessBoard, Piece, bool>? _allowFriendlyFire =
        allowFriendlyFireWhen;
    private readonly Func<IReadOnlyChessBoard, Piece, bool>? _allowNeutralCapture =
        allowNeutralCaptureWhen;

    public CaptureRule(params IMovementBehaviour[] movementBehaviours)
        : this(null, null, movementBehaviours) { }

    public static CaptureRule WithFriendlyFire(
        Func<IReadOnlyChessBoard, Piece, bool> allowFriendlyFireWhen,
        params IMovementBehaviour[] movementBehaviours
    ) => new(allowFriendlyFireWhen, null, movementBehaviours);

    public static CaptureRule WithNeutralCapture(
        Func<IReadOnlyChessBoard, Piece, bool> allowCaptureWhen,
        params IMovementBehaviour[] movementBehaviours
    ) => new(null, allowCaptureWhen, movementBehaviours);

    public IEnumerable<Move> Evaluate(
        IReadOnlyChessBoard board,
        AlgebraicPoint position,
        Piece movingPiece
    )
    {
        foreach (var behaviour in _movementBehaviour)
        {
            foreach (var destination in behaviour.Evaluate(board, position, movingPiece))
            {
                var occupantPiece = board.PeekPieceAt(destination);
                if (occupantPiece is not null && !CanCapture(board, movingPiece, occupantPiece))
                    continue;

                yield return new Move(
                    position,
                    destination,
                    movingPiece,
                    captures: occupantPiece is not null
                        ? [new MoveCapture(destination, board)]
                        : null
                );
            }
        }
    }

    private bool CanCapture(IReadOnlyChessBoard board, Piece mover, Piece target)
    {
        if (mover.Color is null)
            return _allowNeutralCapture?.Invoke(board, target) ?? true;

        bool isFriendly = mover.Color == target.Color;
        if (!isFriendly)
            return true;

        return _allowFriendlyFire?.Invoke(board, target) ?? false;
    }
}
