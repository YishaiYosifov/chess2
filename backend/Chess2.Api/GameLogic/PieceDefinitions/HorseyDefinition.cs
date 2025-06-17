using Chess2.Api.GameLogic.Models;
using Chess2.Api.GameLogic.MovementBehaviours;
using Chess2.Api.GameLogic.PieceMovementRules;

namespace Chess2.Api.GameLogic.PieceDefinitions;

public class HorseyDefinition : IPieceDefinition
{
    public PieceType Type => PieceType.Horsey;

    private readonly List<IPieceMovementRule> _behaviours =
    [
        new CaptureRule(new StepBehaviour(new Offset(X: 1, Y: 2))),
        new CaptureRule(new StepBehaviour(new Offset(X: -1, Y: 2))),
        new CaptureRule(new StepBehaviour(new Offset(X: 1, Y: -2))),
        new CaptureRule(new StepBehaviour(new Offset(X: -1, Y: -2))),
        new CaptureRule(new StepBehaviour(new Offset(X: 2, Y: 1))),
        new CaptureRule(new StepBehaviour(new Offset(X: -2, Y: 1))),
        new CaptureRule(new StepBehaviour(new Offset(X: 2, Y: -1))),
        new CaptureRule(new StepBehaviour(new Offset(X: -2, Y: -1))),
    ];

    public IEnumerable<IPieceMovementRule> GetBehaviours(
        ChessBoard board,
        AlgebraicPoint position,
        Piece movingPiece
    ) => _behaviours;
}
