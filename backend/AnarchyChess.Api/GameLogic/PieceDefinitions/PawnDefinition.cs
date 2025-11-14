using AnarchyChess.Api.GameLogic.Models;
using AnarchyChess.Api.GameLogic.PieceMovementRules;

namespace AnarchyChess.Api.GameLogic.PieceDefinitions;

public class PawnDefinition : BasePawnDefinition
{
    public override PieceType Type => PieceType.Pawn;

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
            maxInitialMoveDistance: 3,
            promotesTo: GameLogicConstants.PromotablePieces
        );
}
