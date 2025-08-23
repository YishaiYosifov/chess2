using Chess2.Api.GameLogic.Models;
using Chess2.Api.GameLogic.MovementBehaviours;
using Chess2.Api.GameLogic.PieceMovementRules;

namespace Chess2.Api.GameLogic.PieceDefinitions;

public class QueenDefinition : IPieceDefinition
{
    public PieceType Type => PieceType.Queen;

    private readonly IPieceMovementRule[] _behaviours =
    [
        new CaptureRule(
            new SlideBehaviour(new Offset(X: 0, Y: 1)),
            new SlideBehaviour(new Offset(X: 0, Y: -1)),
            new SlideBehaviour(new Offset(X: 1, Y: 1)),
            new SlideBehaviour(new Offset(X: 1, Y: 0)),
            new SlideBehaviour(new Offset(X: 1, Y: -1)),
            new SlideBehaviour(new Offset(X: -1, Y: 1)),
            new SlideBehaviour(new Offset(X: -1, Y: 0)),
            new SlideBehaviour(new Offset(X: -1, Y: -1))
        ),
    ];

    public IEnumerable<IPieceMovementRule> GetBehaviours(
        ChessBoard board,
        AlgebraicPoint position,
        Piece movingPiece,
        GameColor movingPlayer
    ) => _behaviours;
}
