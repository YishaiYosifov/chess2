using Chess2.Api.GameLogic.Models;

namespace Chess2.Api.GameLogic.MovementBehaviours;

public class ConditionalBehaviour(
    Func<ChessBoard, AlgebraicPoint, Piece, bool> predicate,
    IMovementBehaviour? trueBranch,
    IMovementBehaviour? falseBranch
) : IMovementBehaviour
{
    private readonly Func<ChessBoard, AlgebraicPoint, Piece, bool> _predicate = predicate;
    private readonly IMovementBehaviour? _trueBranch = trueBranch;
    private readonly IMovementBehaviour? _falseBranch = falseBranch;

    public IEnumerable<AlgebraicPoint> Evaluate(
        ChessBoard board,
        AlgebraicPoint position,
        Piece movingPiece
    ) =>
        _predicate(board, position, movingPiece)
            ? _trueBranch?.Evaluate(board, position, movingPiece) ?? []
            : _falseBranch?.Evaluate(board, position, movingPiece) ?? [];
}
