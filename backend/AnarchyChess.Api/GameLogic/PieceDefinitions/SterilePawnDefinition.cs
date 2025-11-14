using AnarchyChess.Api.GameLogic.Models;
using AnarchyChess.Api.GameLogic.PieceMovementRules;

namespace AnarchyChess.Api.GameLogic.PieceDefinitions;

public class SterilePawnDefinition : BasePawnDefinition
{
    public override PieceType Type => PieceType.SterilePawn;

    private static readonly IReadOnlyCollection<PieceType> _promotesTo =
    [
        .. GameLogicConstants.PromotablePieces.Where(x => x is not PieceType.Queen),
    ];

    public override IEnumerable<IPieceMovementRule> GetBehaviours(
        IReadOnlyChessBoard board,
        AlgebraicPoint position,
        Piece movingPiece,
        GameColor movingPlayer
    ) =>
        GetPawnBehaviours(
            board,
            position,
            movingPiece,
            maxInitialMoveDistance: 1,
            promotesTo: _promotesTo
        );
}
