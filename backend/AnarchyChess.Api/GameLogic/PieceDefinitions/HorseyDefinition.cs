using AnarchyChess.Api.GameLogic.Models;
using AnarchyChess.Api.GameLogic.MovementBehaviours;
using AnarchyChess.Api.GameLogic.PieceMovementRules;

namespace AnarchyChess.Api.GameLogic.PieceDefinitions;

public class HorseyDefinition : IPieceDefinition
{
    public PieceType Type => PieceType.Horsey;

    private readonly IPieceMovementRule[] _behaviours =
    [
        new KnooklearFusionRule(
            fuseWith: PieceType.Rook,
            new CaptureRule(
                allowFriendlyFireWhen: (board, piece) => piece.Type is PieceType.Rook,
                movementBehaviours:
                [
                    new StepBehaviour(new Offset(X: 1, Y: 2)),
                    new StepBehaviour(new Offset(X: -1, Y: 2)),
                    new StepBehaviour(new Offset(X: 1, Y: -2)),
                    new StepBehaviour(new Offset(X: -1, Y: -2)),
                    new StepBehaviour(new Offset(X: 2, Y: 1)),
                    new StepBehaviour(new Offset(X: -2, Y: 1)),
                    new StepBehaviour(new Offset(X: 2, Y: -1)),
                    new StepBehaviour(new Offset(X: -2, Y: -1)),
                ]
            )
        ),
    ];

    public IEnumerable<IPieceMovementRule> GetBehaviours(
        IReadOnlyChessBoard board,
        AlgebraicPoint position,
        Piece movingPiece,
        GameColor movingPlayer
    ) => _behaviours;
}
