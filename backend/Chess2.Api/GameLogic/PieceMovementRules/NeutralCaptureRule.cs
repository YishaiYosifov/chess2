using Chess2.Api.GameLogic.Models;
using Chess2.Api.GameLogic.MovementBehaviours;

namespace Chess2.Api.GameLogic.PieceMovementRules;

public class NeutralCaptureRule(GameColor takeSide, params IMovementBehaviour[] movementBehaviours)
    : IPieceMovementRule
{
    private readonly IMovementBehaviour[] _movementBehaviours = movementBehaviours;
    private readonly GameColor _takeSide = takeSide;

    public IEnumerable<Move> Evaluate(ChessBoard board, AlgebraicPoint position, Piece movingPiece)
    {
        foreach (var behaviour in _movementBehaviours)
        {
            foreach (var destination in behaviour.Evaluate(board, position, movingPiece))
            {
                var occupantPiece = board.PeekPieceAt(destination);

                if (occupantPiece is not null && occupantPiece.Color == _takeSide)
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
