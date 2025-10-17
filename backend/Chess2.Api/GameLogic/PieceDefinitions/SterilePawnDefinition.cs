using Chess2.Api.GameLogic.Models;
using Chess2.Api.GameLogic.PieceMovementRules;

namespace Chess2.Api.GameLogic.PieceDefinitions;

public class SterilePawnDefinition : BasePawnDefinition
{
    public override PieceType Type => PieceType.SterilePawn;

    public override IEnumerable<IPieceMovementRule> GetBehaviours(
        ChessBoard board,
        AlgebraicPoint position,
        Piece movingPiece,
        GameColor movingPlayer
    ) => GetPawnBehaviours(board, movingPiece, maxInitialMoveDistance: 1, canPromote: false);
}
