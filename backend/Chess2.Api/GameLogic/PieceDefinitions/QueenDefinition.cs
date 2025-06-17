using Chess2.Api.GameLogic.Models;
using Chess2.Api.GameLogic.MovementBehaviours;
using Chess2.Api.GameLogic.PieceBehaviours;

namespace Chess2.Api.GameLogic.PieceDefinitions;

public class QueenDefinition : IPieceDefinition
{
    public PieceType Type => PieceType.Queen;

    private readonly List<IPieceBehaviour> _behaviours =
    [
        new CaptureBehaviour(new SlideBehaviour(new Offset(X: 0, Y: 1))),
        new CaptureBehaviour(new SlideBehaviour(new Offset(X: 0, Y: -1))),
        new CaptureBehaviour(new SlideBehaviour(new Offset(X: 1, Y: 1))),
        new CaptureBehaviour(new SlideBehaviour(new Offset(X: 1, Y: 0))),
        new CaptureBehaviour(new SlideBehaviour(new Offset(X: 1, Y: -1))),
        new CaptureBehaviour(new SlideBehaviour(new Offset(X: -1, Y: 1))),
        new CaptureBehaviour(new SlideBehaviour(new Offset(X: -1, Y: 0))),
        new CaptureBehaviour(new SlideBehaviour(new Offset(X: -1, Y: -1))),
    ];

    public IEnumerable<IPieceBehaviour> GetBehaviours(
        ChessBoard board,
        AlgebraicPoint position,
        Piece movingPiece
    ) => _behaviours;
}
