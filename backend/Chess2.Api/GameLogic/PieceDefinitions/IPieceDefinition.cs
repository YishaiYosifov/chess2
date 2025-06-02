using Chess2.Api.GameLogic.Models;
using Chess2.Api.GameLogic.PieceBehaviours;

namespace Chess2.Api.GameLogic.PieceDefinitions;

public interface IPieceDefinition
{
    public PieceType Type { get; }

    public IEnumerable<IPieceBehaviour> GetBehaviours(
        ChessBoard board,
        Point position,
        Piece movingPiece
    );
}
