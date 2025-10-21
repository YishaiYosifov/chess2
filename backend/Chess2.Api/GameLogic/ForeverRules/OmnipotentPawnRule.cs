using Chess2.Api.GameLogic.Extensions;
using Chess2.Api.GameLogic.Models;

namespace Chess2.Api.GameLogic.ForeverRules;

public class OmnipotentPawnRule : IForeveRule
{
    public IEnumerable<Move> GetBehaviours(IReadOnlyChessBoard board, GameColor movingPlayer)
    {
        if (board.Moves.Count == 0)
            yield break;
        var lastMove = board.Moves[^1];

        var spawnPosition = movingPlayer.Match(
            whenWhite: new AlgebraicPoint("h3"),
            whenBlack: new AlgebraicPoint("h8")
        );
        if (
            lastMove.To != spawnPosition
            || !lastMove.Captures.Any(capture => capture.CapturedPiece.Color == movingPlayer)
        )
            yield break;

        yield return new Move(
            from: lastMove.To,
            to: lastMove.To,
            piece: lastMove.Piece,
            captures: [new MoveCapture(lastMove.To, board)],
            pieceSpawns: [new PieceSpawn(PieceType.Pawn, Color: movingPlayer, lastMove.To)],
            specialMoveType: SpecialMoveType.OmnipotentPawnSpawn
        );
    }
}
