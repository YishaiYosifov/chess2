using AnarchyChess.Api.GameLogic.Models;
using AnarchyChess.Api.GameLogic.PieceMovementRules;

namespace AnarchyChess.Api.GameLogic.PieceDefinitions;

public class UnderagePawnDefinition : BasePawnDefinition
{
    public override PieceType Type => PieceType.UnderagePawn;

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
            maxInitialMoveDistance: 2,
            promotesTo: GameLogicConstants.PromotablePieces
        );
}
