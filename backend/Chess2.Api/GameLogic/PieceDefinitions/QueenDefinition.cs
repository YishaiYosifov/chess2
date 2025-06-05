using Chess2.Api.GameLogic.Models;
using Chess2.Api.GameLogic.MovementBehaviours;
using Chess2.Api.GameLogic.PieceBehaviours;

namespace Chess2.Api.GameLogic.PieceDefinitions;

public class QueenDefinition : IPieceDefinition
{
    public PieceType Type => PieceType.King;

    private readonly List<IPieceBehaviour> _behaviours =
    [
        new CaptureBehaviour(new SlideBehaviour(new Point(X: 0, Y: 1))),
        new CaptureBehaviour(new SlideBehaviour(new Point(X: 0, Y: -1))),
        new CaptureBehaviour(new SlideBehaviour(new Point(X: 1, Y: 0))),
        new CaptureBehaviour(new SlideBehaviour(new Point(X: 1, Y: 1))),
        new CaptureBehaviour(new SlideBehaviour(new Point(X: 1, Y: -1))),
        new CaptureBehaviour(new SlideBehaviour(new Point(X: -1, Y: 1))),
        new CaptureBehaviour(new SlideBehaviour(new Point(X: -1, Y: -1))),
    ];

    public IEnumerable<IPieceBehaviour> GetBehaviours(
        ChessBoard board,
        Point position,
        Piece movingPiece
    ) => _behaviours;
}
