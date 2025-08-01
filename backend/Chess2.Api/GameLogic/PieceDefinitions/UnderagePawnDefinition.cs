using Chess2.Api.GameLogic.Models;
using Chess2.Api.GameLogic.PieceMovementRules;

namespace Chess2.Api.GameLogic.PieceDefinitions;

public class UnderagePawnDefinition : BasePawnDefinition
{
    public override PieceType Type => PieceType.UnderagePawn;

    public override IEnumerable<IPieceMovementRule> GetBehaviours(
        ChessBoard board,
        AlgebraicPoint position,
        Piece movingPiece
    ) => GetPawnBehaviours(board, movingPiece, maxInitialMoveDistance: 2);
}
