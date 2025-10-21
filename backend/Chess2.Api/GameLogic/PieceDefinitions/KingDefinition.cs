using Chess2.Api.GameLogic.Models;
using Chess2.Api.GameLogic.MovementBehaviours;
using Chess2.Api.GameLogic.PieceMovementRules;

namespace Chess2.Api.GameLogic.PieceDefinitions;

public class KingDefinition : IPieceDefinition
{
    public PieceType Type => PieceType.King;

    private readonly IPieceMovementRule[] _behaviours =
    [
        new CaptureRule(
            new StepBehaviour(new Offset(X: 0, Y: 1)),
            new StepBehaviour(new Offset(X: 0, Y: -1)),
            new StepBehaviour(new Offset(X: 1, Y: 1)),
            new StepBehaviour(new Offset(X: 1, Y: 0)),
            new StepBehaviour(new Offset(X: 1, Y: -1)),
            new StepBehaviour(new Offset(X: -1, Y: 1)),
            new StepBehaviour(new Offset(X: -1, Y: 0)),
            new StepBehaviour(new Offset(X: -1, Y: -1))
        ),
        new CastleRule(),
    ];

    public IEnumerable<IPieceMovementRule> GetBehaviours(
        IReadOnlyChessBoard board,
        AlgebraicPoint position,
        Piece movingPiece,
        GameColor movingPlayer
    ) => _behaviours;
}
