using Chess2.Api.GameLogic.Models;
using Chess2.Api.GameLogic.MovementBehaviours;
using Chess2.Api.GameLogic.PieceMovementRules;

namespace Chess2.Api.GameLogic.PieceDefinitions;

public class BishopDefinition : IPieceDefinition
{
    public PieceType Type => PieceType.Bishop;

    private readonly IPieceMovementRule[] _behaviours =
    [
        CreateForcedCaptureRuleFor(new Offset(X: 1, Y: 1)),
        CreateForcedCaptureRuleFor(new Offset(X: 1, Y: -1)),
        CreateForcedCaptureRuleFor(new Offset(X: -1, Y: 1)),
        CreateForcedCaptureRuleFor(new Offset(X: -1, Y: -1)),
    ];

    public IEnumerable<IPieceMovementRule> GetBehaviours(
        ChessBoard board,
        AlgebraicPoint position,
        Piece movingPiece,
        GameColor movingPlayer
    ) => _behaviours;

    private static ForcedMoveRule CreateForcedCaptureRuleFor(Offset offset) =>
        new(
            priority: ForcedMovePriority.UnderagePawn,
            predicate: (board, move) =>
                move.CapturedSquares.Any(c =>
                    board.TryGetPieceAt(c, out var capture)
                    && capture.Type == PieceType.UnderagePawn
                ),
            new CaptureRule(
                allowFriendlyFire: (board, piece) => piece.Type == PieceType.UnderagePawn,
                new SlideBehaviour(offset)
            )
        );
}
