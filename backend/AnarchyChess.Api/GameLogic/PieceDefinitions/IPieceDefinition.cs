using AnarchyChess.Api.GameLogic.Models;
using AnarchyChess.Api.GameLogic.PieceMovementRules;

namespace AnarchyChess.Api.GameLogic.PieceDefinitions;

public interface IPieceDefinition
{
    PieceType Type { get; }

    IEnumerable<IPieceMovementRule> GetBehaviours(
        IReadOnlyChessBoard board,
        AlgebraicPoint position,
        Piece movingPiece,
        GameColor movingPlayer
    );
}
