using Chess2.Api.GameLogic.Models;
using Chess2.Api.GameLogic.MovementBehaviours;
using Chess2.Api.GameLogic.PieceMovementRules;

namespace Chess2.Api.GameLogic.PieceDefinitions;

public class TraitorRookDefinition : IPieceDefinition
{
    public PieceType Type => PieceType.TraitorRook;

    private readonly List<IPieceMovementRule> _behaviours =
    [
        new NeutralRule(new SlideBehaviour(new Offset(X: 0, Y: 1))),
        new NeutralRule(new SlideBehaviour(new Offset(X: 0, Y: -1))),
        new NeutralRule(new SlideBehaviour(new Offset(X: 1, Y: 0))),
        new NeutralRule(new SlideBehaviour(new Offset(X: -1, Y: 0))),
    ];

    public IEnumerable<IPieceMovementRule> GetBehaviours(
        ChessBoard board,
        AlgebraicPoint position,
        Piece movingPiece
    ) => _behaviours;
}
