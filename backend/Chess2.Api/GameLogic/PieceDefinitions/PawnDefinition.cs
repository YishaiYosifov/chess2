using Chess2.Api.GameLogic.Models;
using Chess2.Api.GameLogic.MovementBehaviours;
using Chess2.Api.GameLogic.PieceBehaviours;

namespace Chess2.Api.GameLogic.PieceDefinitions;

public class PawnDefinition : IPieceDefinition
{
    public PieceType Type => PieceType.Pawn;

    public IEnumerable<IPieceBehaviour> GetBehaviours(
        ChessBoard board,
        AlgebraicPoint position,
        Piece movingPiece
    )
    {
        var direction = movingPiece.Color == GameColor.White ? 1 : -1;

        IEnumerable<IPieceBehaviour> behaviours =
        [
            new NoCaptureBehaviour(
                new ConditionalBehaviour(
                    (board, pos, piece) => piece.TimesMoved == 0,
                    // move 3 squares if this piece has not moved before
                    trueBranch: new SlideBehaviour(new Offset(X: 0, Y: 1 * direction), max: 3),
                    // move one square if this piece has moved before
                    falseBranch: new StepBehaviour(new Offset(X: 0, Y: 1 * direction))
                )
            ),
            new CaptureOnlyBehaviour(new StepBehaviour(new Offset(X: 1, Y: 1 * direction))),
            new CaptureOnlyBehaviour(new StepBehaviour(new Offset(X: -1, Y: 1 * direction))),
            new EnPassantBehaviour(new Offset(X: 1, Y: 1 * direction)),
            new EnPassantBehaviour(new Offset(X: -1, Y: 1 * direction)),
        ];

        return behaviours;
    }
}
