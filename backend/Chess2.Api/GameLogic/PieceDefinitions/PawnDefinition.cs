using Chess2.Api.GameLogic.Models;
using Chess2.Api.GameLogic.MovementBehaviours;
using Chess2.Api.GameLogic.PieceBehaviours;

namespace Chess2.Api.GameLogic.PieceDefinitions;

public class PawnDefinition : IPieceDefinition
{
    public PieceType Type => PieceType.Pawn;

    public IEnumerable<IPieceBehaviour> GetBehaviours(
        ChessBoard board,
        Point position,
        Piece movingPiece
    )
    {
        var direction = movingPiece.Color == GameColor.White ? 1 : -1;

        IEnumerable<IPieceBehaviour> behaviours =
        [
            new NoCaptureBehaviour(new StepBehaviour(new Point(X: 0, Y: 1 * direction))),
            new CaptureOnlyBehaviour(new StepBehaviour(new Point(X: 1, Y: 1 * direction))),
            new CaptureOnlyBehaviour(new StepBehaviour(new Point(X: -1, Y: 1 * direction))),
            new EnPassantBehaviour(new Point(X: 1, Y: 1 * direction)),
            new EnPassantBehaviour(new Point(X: -1, Y: 1 * direction)),
            new NoCaptureBehaviour(
                new TimesMovedRestrictedBehaviour(
                    new StepBehaviour(new Point(X: 0, Y: 2 * direction)),
                    maxTimesMoved: 0
                )
            ),
        ];

        return behaviours;
    }
}
