using Chess2.Api.GameLogic.Models;

namespace Chess2.Api.GameLogic.PieceBehaviours;

public class EnPassantBehaviour(Point offset) : IPieceBehaviour
{
    private readonly Point _offset = offset;

    private const PieceType EnPassantType = PieceType.Pawn;

    public IEnumerable<Move> Evaluate(ChessBoard board, Point position, Piece movingPiece)
    {
        var targetPos = position + _offset;
        if (!board.IsWithinBoundaries(targetPos))
            yield break;

        var lastMove = board.LastMove;
        if (
            lastMove is null
            || lastMove.Piece.Type != EnPassantType
            || lastMove.Piece.Color == movingPiece.Color
        )
            yield break;

        var distanceLastPieceMoved = Math.Abs(lastMove.From.Y - lastMove.To.Y);
        if (distanceLastPieceMoved < 2)
            yield break;

        var distanceBetweenFiles = Math.Abs(lastMove.To.X - targetPos.X);
        if (distanceBetweenFiles != 1)
            yield break;

        // check if the target position is between last move From and To positions
        if (
            targetPos.Y < Math.Min(lastMove.From.Y, lastMove.To.Y)
            || targetPos.Y > Math.Max(lastMove.From.Y, lastMove.To.Y)
        )
            yield break;

        yield return new Move(position, targetPos, movingPiece, CapturedSquares: [lastMove.To]);
    }
}
