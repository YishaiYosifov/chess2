using Chess2.Api.GameLogic.Models;
using Chess2.Api.GameLogic.PieceMovementRules;

namespace Chess2.Api.GameLogic.PieceDefinitions;

public class ChildPawnDefinition : BasePawnDefinition
{
    public override PieceType Type => PieceType.ChildPawn;

    public override IEnumerable<IPieceMovementRule> GetBehaviours(
        ChessBoard board,
        AlgebraicPoint position,
        Piece movingPiece
    ) => GetPawnBehaviours(movingPiece, maxInitialMoveDistance: 2);
}
