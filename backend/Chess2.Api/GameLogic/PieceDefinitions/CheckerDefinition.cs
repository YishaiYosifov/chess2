using Chess2.Api.GameLogic.Models;
using Chess2.Api.GameLogic.MovementBehaviours;
using Chess2.Api.GameLogic.PieceMovementRules;

namespace Chess2.Api.GameLogic.PieceDefinitions;

public class CheckerDefinition : IPieceDefinition
{
    public PieceType Type => PieceType.Checker;

    private readonly IPieceMovementRule[] _behaviours =
    [
        new NoCaptureRule(
            new SlideBehaviour(new Offset(X: 1, Y: 1), max: 2),
            new SlideBehaviour(new Offset(X: -1, Y: 1), max: 2),
            new SlideBehaviour(new Offset(X: 1, Y: -1), max: 2),
            new SlideBehaviour(new Offset(X: -1, Y: -1), max: 2)
        ),
        new CheckerCaptureRule(
            [
                new Offset(X: 1, Y: 1),
                new Offset(X: -1, Y: 1),
                new Offset(X: 1, Y: -1),
                new Offset(X: -1, Y: -1),
            ]
        ),
    ];

    public IEnumerable<IPieceMovementRule> GetBehaviours(
        ChessBoard board,
        AlgebraicPoint position,
        Piece movingPiece,
        GameColor movingPlayer
    ) => _behaviours;
}
