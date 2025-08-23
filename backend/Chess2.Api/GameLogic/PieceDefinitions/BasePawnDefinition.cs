using Chess2.Api.GameLogic.Extensions;
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
        Piece movingPiece,
        GameColor movingPlayer
    );

    protected IEnumerable<IPieceMovementRule> GetPawnBehaviours(
        ChessBoard board,
        Piece movingPiece,
        int maxInitialMoveDistance
    )
    {
        if (movingPiece.Color is null)
            yield break;

        var color = movingPiece.Color.Value;
        var direction = color.Match(whenWhite: 1, whenBlack: -1);
        int promotionY = color.Match(whenWhite: board.Height - 1, whenBlack: 0);
        yield return new PromotionRule(
            (_, move) => move.To.Y == promotionY,
            new NoCaptureRule(
                new ConditionalBehaviour(
                    (board, pos, piece) => piece.TimesMoved == 0,
                    // move maxInitialMoveDistance squares if this piece has not moved before
                    trueBranch: new SlideBehaviour(
                        new Offset(X: 0, Y: 1 * direction),
                        max: maxInitialMoveDistance
                    ),
                    // move one square if this piece has moved before
                    falseBranch: new StepBehaviour(new Offset(X: 0, Y: 1 * direction))
                )
            ),
            new CaptureOnlyRule(
                new StepBehaviour(new Offset(X: 1, Y: 1 * direction)),
                new StepBehaviour(new Offset(X: -1, Y: 1 * direction))
            ),
            new EnPassantRule(
                direction: new Offset(X: 1, Y: 1 * direction),
                chainCaptureDirection: new Offset(X: 0, Y: -direction)
            ),
            new EnPassantRule(
                direction: new Offset(X: -1, Y: 1 * direction),
                chainCaptureDirection: new Offset(X: 0, Y: -direction)
            )
        );
    }
}
