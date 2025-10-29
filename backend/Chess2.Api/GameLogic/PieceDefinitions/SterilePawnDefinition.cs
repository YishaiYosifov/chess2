using Chess2.Api.GameLogic.Models;
using Chess2.Api.GameLogic.PieceMovementRules;

namespace Chess2.Api.GameLogic.PieceDefinitions;

public class SterilePawnDefinition : BasePawnDefinition
{
    public override PieceType Type => PieceType.SterilePawn;

    private static IReadOnlyCollection<PieceType> _promotesTo =
    [
        .. GameLogicConstants.PromotablePieces.Where(x => x is not PieceType.Queen),
    ];

    public override IEnumerable<IPieceMovementRule> GetBehaviours(
        IReadOnlyChessBoard board,
        AlgebraicPoint position,
        Piece movingPiece,
        GameColor movingPlayer
    ) => GetPawnBehaviours(board, movingPiece, maxInitialMoveDistance: 1, promotesTo: _promotesTo);
}
