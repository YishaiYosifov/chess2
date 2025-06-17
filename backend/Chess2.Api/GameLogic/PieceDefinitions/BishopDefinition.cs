using Chess2.Api.GameLogic.Models;
using Chess2.Api.GameLogic.MovementBehaviours;
using Chess2.Api.GameLogic.PieceBehaviours;

namespace Chess2.Api.GameLogic.PieceDefinitions;

public class BishopDefinition : IPieceDefinition
{
    public PieceType Type => PieceType.Bishop;

    private readonly List<IPieceBehaviour> _behaviours =
    [
        new CaptureBehaviour(new SlideBehaviour(new Offset(X: 1, Y: 1))),
        new CaptureBehaviour(new StepBehaviour(new Offset(X: 1, Y: -1))),
        new CaptureBehaviour(new StepBehaviour(new Offset(X: -1, Y: 1))),
        new CaptureBehaviour(new StepBehaviour(new Offset(X: -1, Y: -1))),
    ];

    public IEnumerable<IPieceBehaviour> GetBehaviours(
        ChessBoard board,
        AlgebraicPoint position,
        Piece movingPiece
    ) => _behaviours;
}
