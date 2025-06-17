using Chess2.Api.GameLogic.Models;
using Chess2.Api.GameLogic.MovementBehaviours;
using Chess2.Api.GameLogic.PieceBehaviours;

namespace Chess2.Api.GameLogic.PieceDefinitions;

public class HorseyDefinition : IPieceDefinition
{
    public PieceType Type => PieceType.Horsey;

    private readonly List<IPieceBehaviour> _behaviours =
    [
        new CaptureBehaviour(new StepBehaviour(new Offset(X: 1, Y: 2))),
        new CaptureBehaviour(new StepBehaviour(new Offset(X: -1, Y: 2))),
        new CaptureBehaviour(new StepBehaviour(new Offset(X: 1, Y: -2))),
        new CaptureBehaviour(new StepBehaviour(new Offset(X: -1, Y: -2))),
        new CaptureBehaviour(new StepBehaviour(new Offset(X: 2, Y: 1))),
        new CaptureBehaviour(new StepBehaviour(new Offset(X: -2, Y: 1))),
        new CaptureBehaviour(new StepBehaviour(new Offset(X: 2, Y: -1))),
        new CaptureBehaviour(new StepBehaviour(new Offset(X: -2, Y: -1))),
    ];

    public IEnumerable<IPieceBehaviour> GetBehaviours(
        ChessBoard board,
        AlgebraicPoint position,
        Piece movingPiece
    ) => _behaviours;
}
