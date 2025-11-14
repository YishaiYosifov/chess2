using AnarchyChess.Api.GameLogic.Models;
using AnarchyChess.Api.GameLogic.MovementBehaviours;

namespace AnarchyChess.Api.GameLogic.PieceMovementRules;

public class NoCaptureRule(params IMovementBehaviour[] movementBehaviours) : IPieceMovementRule
{
    private readonly IMovementBehaviour[] _movementBehaviours = movementBehaviours;

    public IEnumerable<Move> Evaluate(
        IReadOnlyChessBoard board,
        AlgebraicPoint position,
        Piece movingPiece
    )
    {
        foreach (var behaviour in _movementBehaviours)
        {
            foreach (var destination in behaviour.Evaluate(board, position, movingPiece))
            {
                var isSquareEmpty = board.IsEmpty(destination);
                if (!isSquareEmpty)
                    continue;

                yield return new Move(position, destination, movingPiece);
            }
        }
    }
}
