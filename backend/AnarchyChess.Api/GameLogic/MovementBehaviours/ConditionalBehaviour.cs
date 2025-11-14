using AnarchyChess.Api.GameLogic.Models;

namespace AnarchyChess.Api.GameLogic.MovementBehaviours;

public class ConditionalBehaviour(
    Func<IReadOnlyChessBoard, AlgebraicPoint, Piece, bool> predicate,
    IMovementBehaviour? trueBranch,
    IMovementBehaviour? falseBranch
) : IMovementBehaviour
{
    private readonly Func<IReadOnlyChessBoard, AlgebraicPoint, Piece, bool> _predicate = predicate;
    private readonly IMovementBehaviour? _trueBranch = trueBranch;
    private readonly IMovementBehaviour? _falseBranch = falseBranch;

    public IEnumerable<AlgebraicPoint> Evaluate(
        IReadOnlyChessBoard board,
        AlgebraicPoint position,
        Piece movingPiece
    ) =>
        _predicate(board, position, movingPiece)
            ? _trueBranch?.Evaluate(board, position, movingPiece) ?? []
            : _falseBranch?.Evaluate(board, position, movingPiece) ?? [];
}
