using Chess2.Api.GameLogic.Models;
using Chess2.Api.GameLogic.PieceMovementRules;

namespace Chess2.Api.GameLogic.PieceDefinitions;

public interface IPieceDefinition
{
    public PieceType Type { get; }

    public IEnumerable<IPieceMovementRule> GetBehaviours(
        ChessBoard board,
        AlgebraicPoint position,
        Piece movingPiece
    );
}
