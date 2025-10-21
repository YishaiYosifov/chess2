using Chess2.Api.GameLogic.Models;
using Chess2.Api.GameLogic.PieceMovementRules;

namespace Chess2.Api.GameLogic.PieceDefinitions;

public class PawnDefinition : BasePawnDefinition
{
    public override PieceType Type => PieceType.Pawn;

    public override IEnumerable<IPieceMovementRule> GetBehaviours(
        IReadOnlyChessBoard board,
        AlgebraicPoint position,
        Piece movingPiece,
        GameColor movingPlayer
    ) => GetPawnBehaviours(board, movingPiece, maxInitialMoveDistance: 3);
}
