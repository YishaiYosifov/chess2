using Chess2.Api.GameLogic.Extensions;
using Chess2.Api.GameLogic.Models;
using Chess2.Api.GameLogic.MovementBehaviours;
using Chess2.Api.GameLogic.PieceMovementRules;

namespace Chess2.Api.GameLogic.PieceDefinitions;

public class QueenDefinition : IPieceDefinition
{
    public PieceType Type => PieceType.Queen;

    private readonly IPieceMovementRule _regularQueenMoves = new CaptureRule(
        new SlideBehaviour(new Offset(X: 0, Y: 1)),
        new SlideBehaviour(new Offset(X: 0, Y: -1)),
        new SlideBehaviour(new Offset(X: 1, Y: 1)),
        new SlideBehaviour(new Offset(X: 1, Y: 0)),
        new SlideBehaviour(new Offset(X: 1, Y: -1)),
        new SlideBehaviour(new Offset(X: -1, Y: 1)),
        new SlideBehaviour(new Offset(X: -1, Y: 0)),
        new SlideBehaviour(new Offset(X: -1, Y: -1))
    );

    public IEnumerable<IPieceMovementRule> GetBehaviours(
        IReadOnlyChessBoard board,
        AlgebraicPoint position,
        Piece movingPiece,
        GameColor movingPlayer
    )
    {
        yield return _regularQueenMoves;

        var directionY = movingPlayer.Match(whenWhite: 1, whenBlack: -1);
        yield return new RadioactiveBetaDecayRule(
            new()
            {
                { new Offset(-1, 0), PieceType.Rook },
                { new Offset(1, 0), PieceType.Horsey },
                { new Offset(0, directionY), PieceType.SterilePawn },
            }
        );
    }
}
