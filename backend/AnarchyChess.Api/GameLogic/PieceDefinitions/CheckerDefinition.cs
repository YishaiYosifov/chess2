using AnarchyChess.Api.GameLogic.Extensions;
using AnarchyChess.Api.GameLogic.Models;
using AnarchyChess.Api.GameLogic.MovementBehaviours;
using AnarchyChess.Api.GameLogic.PieceMovementRules;

namespace AnarchyChess.Api.GameLogic.PieceDefinitions;

public class CheckerDefinition : IPieceDefinition
{
    public PieceType Type => PieceType.Checker;

    private readonly IPieceMovementRule[] _behaviours =
    [
        new NoCaptureRule(
            new SlideBehaviour(new Offset(X: 1, Y: 1), max: 2),
            new SlideBehaviour(new Offset(X: -1, Y: 1), max: 2),
            new SlideBehaviour(new Offset(X: 1, Y: -1), max: 2),
            new SlideBehaviour(new Offset(X: -1, Y: -1), max: 2)
        ),
        new CheckerJumpRule(
            new Offset(X: 1, Y: 1),
            new Offset(X: -1, Y: 1),
            new Offset(X: 1, Y: -1),
            new Offset(X: -1, Y: -1)
        ),
    ];

    public IEnumerable<IPieceMovementRule> GetBehaviours(
        IReadOnlyChessBoard board,
        AlgebraicPoint position,
        Piece movingPiece,
        GameColor movingPlayer
    )
    {
        if (movingPiece.Color is null)
            yield break;

        int promotionY = movingPiece.Color.Value.Match(whenWhite: board.Height - 1, whenBlack: 0);
        yield return new PromotionRule(
            (_, move) => move.To.Y == promotionY,
            promotesTo: [PieceType.King],
            _behaviours
        );
    }
}
