using AnarchyChess.Api.GameLogic.Models;
using AnarchyChess.Api.GameLogic.MovementBehaviours;
using AnarchyChess.Api.GameLogic.PieceMovementRules;

namespace AnarchyChess.Api.GameLogic.PieceDefinitions;

public class BishopDefinition : IPieceDefinition
{
    public PieceType Type => PieceType.Bishop;

    private readonly IPieceMovementRule[] _behaviours =
    [
        new ForcedMoveRule(
            priority: ForcedMovePriority.UnderagePawn,
            predicate: (board, move) =>
                move.Captures.Any(c => c.CapturedPiece.Type == PieceType.UnderagePawn),
            CaptureRule.WithFriendlyFire(
                allowFriendlyFireWhen: (board, piece) => piece.Type == PieceType.UnderagePawn,
                new SlideBehaviour(new Offset(X: 1, Y: 1)),
                new SlideBehaviour(new Offset(X: 1, Y: -1)),
                new SlideBehaviour(new Offset(X: -1, Y: 1)),
                new SlideBehaviour(new Offset(X: -1, Y: -1))
            ),
            new IlVaticanoRule(new Offset(-1, 0)),
            new IlVaticanoRule(new Offset(1, 0)),
            new IlVaticanoRule(new Offset(0, -1)),
            new IlVaticanoRule(new Offset(0, 1))
        ),
    ];

    public IEnumerable<IPieceMovementRule> GetBehaviours(
        IReadOnlyChessBoard board,
        AlgebraicPoint position,
        Piece movingPiece,
        GameColor movingPlayer
    ) => _behaviours;
}
