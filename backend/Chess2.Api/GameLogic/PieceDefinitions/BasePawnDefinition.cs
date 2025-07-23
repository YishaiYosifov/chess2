using Chess2.Api.GameLogic.Models;
using Chess2.Api.GameLogic.MovementBehaviours;
using Chess2.Api.GameLogic.PieceMovementRules;

namespace Chess2.Api.GameLogic.PieceDefinitions;

public abstract class BasePawnDefinition : IPieceDefinition
{
    public abstract PieceType Type { get; }

    public abstract IEnumerable<IPieceMovementRule> GetBehaviours(
        ChessBoard board,
        AlgebraicPoint position,
        Piece movingPiece
    );

    protected IEnumerable<IPieceMovementRule> GetPawnBehaviours(
        Piece movingPiece,
        int maxInitialMoveDistance
    )
    {
        var direction = movingPiece.Color == GameColor.White ? 1 : -1;

        IPieceMovementRule[] behaviours =
        [
            new NoCaptureRule(
                new ConditionalBehaviour(
                    (board, pos, piece) => piece.TimesMoved == 0,
                    // move 3 squares if this piece has not moved before
                    trueBranch: new SlideBehaviour(
                        new Offset(X: 0, Y: 1 * direction),
                        max: maxInitialMoveDistance
                    ),
                    // move one square if this piece has moved before
                    falseBranch: new StepBehaviour(new Offset(X: 0, Y: 1 * direction))
                )
            ),
            new CaptureOnlyRule(new StepBehaviour(new Offset(X: 1, Y: 1 * direction))),
            new CaptureOnlyRule(new StepBehaviour(new Offset(X: -1, Y: 1 * direction))),
            new EnPassantRule(new Offset(X: 1, Y: 1 * direction)),
            new EnPassantRule(new Offset(X: -1, Y: 1 * direction)),
        ];

        return behaviours;
    }
}
