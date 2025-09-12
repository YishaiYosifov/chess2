using Chess2.Api.GameLogic.Models;
using Chess2.Api.GameLogic.MovementBehaviours;
using Chess2.Api.GameLogic.PieceMovementRules;

namespace Chess2.Api.GameLogic.PieceDefinitions;

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
            )
        ),
    ];

    public IEnumerable<IPieceMovementRule> GetBehaviours(
        ChessBoard board,
        AlgebraicPoint position,
        Piece movingPiece,
        GameColor movingPlayer
    ) => _behaviours;
}
