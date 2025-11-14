using AnarchyChess.Api.GameLogic.Models;
using AnarchyChess.Api.GameLogic.MovementBehaviours;
using AnarchyChess.Api.GameLogic.PieceMovementRules;

namespace AnarchyChess.Api.GameLogic.PieceDefinitions;

public class RookDefinition : IPieceDefinition
{
    public PieceType Type => PieceType.Rook;

    private readonly IPieceMovementRule[] _behaviours =
    [
        new KnooklearFusionRule(
            fuseWith: PieceType.Horsey,
            new CaptureRule(
                allowFriendlyFireWhen: (board, piece) => piece.Type is PieceType.Horsey,
                movementBehaviours:
                [
                    new SlideBehaviour(new Offset(X: 0, Y: 1)),
                    new SlideBehaviour(new Offset(X: 0, Y: -1)),
                    new SlideBehaviour(new Offset(X: 1, Y: 0)),
                    new SlideBehaviour(new Offset(X: -1, Y: 0)),
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
