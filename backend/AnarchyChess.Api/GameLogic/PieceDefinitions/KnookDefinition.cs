using AnarchyChess.Api.GameLogic.Models;
using AnarchyChess.Api.GameLogic.MovementBehaviours;
using AnarchyChess.Api.GameLogic.PieceMovementRules;

namespace AnarchyChess.Api.GameLogic.PieceDefinitions;

public class KnookDefinition : IPieceDefinition
{
    public PieceType Type => PieceType.Knook;

    private readonly IPieceMovementRule[] _behaviours =
    [
        new CaptureRule(
            // horsey part
            new StepBehaviour(new Offset(X: 1, Y: 2)),
            new StepBehaviour(new Offset(X: -1, Y: 2)),
            new StepBehaviour(new Offset(X: 1, Y: -2)),
            new StepBehaviour(new Offset(X: -1, Y: -2)),
            new StepBehaviour(new Offset(X: 2, Y: 1)),
            new StepBehaviour(new Offset(X: -2, Y: 1)),
            new StepBehaviour(new Offset(X: 2, Y: -1)),
            new StepBehaviour(new Offset(X: -2, Y: -1)),
            // rook part
            new SlideBehaviour(new Offset(X: 0, Y: 1), max: 2),
            new SlideBehaviour(new Offset(X: 0, Y: -1), max: 2),
            new SlideBehaviour(new Offset(X: 1, Y: 0), max: 2),
            new SlideBehaviour(new Offset(X: -1, Y: 0), max: 2)
        ),
    ];

    public IEnumerable<IPieceMovementRule> GetBehaviours(
        IReadOnlyChessBoard board,
        AlgebraicPoint position,
        Piece movingPiece,
        GameColor movingPlayer
    ) => _behaviours;
}
