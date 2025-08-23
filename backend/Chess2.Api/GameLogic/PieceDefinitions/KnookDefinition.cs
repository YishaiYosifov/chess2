using Chess2.Api.GameLogic.Models;
using Chess2.Api.GameLogic.MovementBehaviours;
using Chess2.Api.GameLogic.PieceMovementRules;

namespace Chess2.Api.GameLogic.PieceDefinitions;

public class KnookDefinition : IPieceDefinition
{
    public PieceType Type => PieceType.Knook;

    private readonly IPieceMovementRule[] _behaviours =
    [
        // horsey part
        new CaptureOnlyRule(
            new StepBehaviour(new Offset(X: 1, Y: 2)),
            new StepBehaviour(new Offset(X: -1, Y: 2)),
            new StepBehaviour(new Offset(X: 1, Y: -2)),
            new StepBehaviour(new Offset(X: -1, Y: -2)),
            new StepBehaviour(new Offset(X: 2, Y: 1)),
            new StepBehaviour(new Offset(X: -2, Y: 1)),
            new StepBehaviour(new Offset(X: 2, Y: -1)),
            new StepBehaviour(new Offset(X: -2, Y: -1))
        ),
        // rook part
        new NoCaptureRule(
            new SlideBehaviour(new Offset(X: 0, Y: 1)),
            new SlideBehaviour(new Offset(X: 0, Y: -1)),
            new SlideBehaviour(new Offset(X: 1, Y: 0)),
            new SlideBehaviour(new Offset(X: -1, Y: 0))
        ),
    ];

    public IEnumerable<IPieceMovementRule> GetBehaviours(
        ChessBoard board,
        AlgebraicPoint position,
        Piece movingPiece,
        GameColor movingPlayer
    ) => _behaviours;
}
