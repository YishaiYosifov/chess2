using Chess2.Api.GameLogic.Models;

namespace Chess2.Api.GameLogic.PieceMovementRules;

public class EnPassantRule(Offset direction, Offset chainCaptureDirection) : IPieceMovementRule
{
    private readonly Offset _direction = direction;
    private readonly Offset _chainCaptureDirection = chainCaptureDirection;

    private readonly HashSet<PieceType> EnPassantType = [PieceType.Pawn, PieceType.UnderagePawn];

    public IEnumerable<Move> Evaluate(ChessBoard board, AlgebraicPoint position, Piece movingPiece)
    {
        var pawnEnPassant = EvaluatePawnEnPassant(board, position, movingPiece);
        if (pawnEnPassant is null)
            yield break;

        yield return pawnEnPassant;
        foreach (var move in EvaluateEnPassantChain(board, movingPiece, pawnEnPassant))
        {
            yield return move;
        }
    }

    private Move? EvaluatePawnEnPassant(
        ChessBoard board,
        AlgebraicPoint position,
        Piece movingPiece
    )
    {
        var targetPos = position + _direction;
        if (!board.IsWithinBoundaries(targetPos))
            return null;

        var lastMove = board.Moves.Count > 0 ? board.Moves[^1] : null;
        if (
            lastMove is null
            || !EnPassantType.Contains(lastMove.Piece.Type)
            || lastMove.Piece.Color == movingPiece.Color
            || lastMove.Piece.TimesMoved != 0
        )
            return null;

        var distanceLastPieceMoved = Math.Abs(lastMove.From.Y - lastMove.To.Y);
        if (distanceLastPieceMoved < 2)
            return null;

        var distanceBetweenFiles = Math.Abs(lastMove.To.X - targetPos.X);
        if (distanceBetweenFiles != 0)
            return null;

        // check if the target position is between last move From and To positions
        if (
            targetPos.Y <= Math.Min(lastMove.From.Y, lastMove.To.Y)
            || targetPos.Y >= Math.Max(lastMove.From.Y, lastMove.To.Y)
        )
            return null;

        return new Move(
            position,
            targetPos,
            movingPiece,
            captures: [new MoveCapture(lastMove.To, board)],
            forcedPriority: ForcedMovePriority.EnPassant
        );
    }

    private IEnumerable<Move> EvaluateEnPassantChain(
        ChessBoard board,
        Piece movingPiece,
        Move pawnEnPassant
    )
    {
        var from = pawnEnPassant.From;
        var lastMoveTo = pawnEnPassant.To;

        List<MoveCapture> capturedSquares = [.. pawnEnPassant.Captures];
        while (true)
        {
            var to = lastMoveTo + _direction;
            var capturePosition = to + _chainCaptureDirection;

            if (!board.IsWithinBoundaries(to))
                yield break;

            if (board.PeekPieceAt(to) is not null)
                yield break;

            if (
                !board.TryGetPieceAt(capturePosition, out var capture)
                || capture.Color == movingPiece.Color
            )
                yield break;

            capturedSquares.Add(new MoveCapture(capturePosition, board));
            yield return new Move(
                from,
                to,
                movingPiece,
                captures: [.. capturedSquares],
                forcedPriority: ForcedMovePriority.EnPassant
            );

            lastMoveTo = to;
        }
    }
}
