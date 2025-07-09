using Chess2.Api.GameLogic.Models;

namespace Chess2.Api.GameLogic.PieceMovementRules;

public class CastleRule : IPieceMovementRule
{
    private const int KingCastlingStepCount = 2;
    private readonly PieceType _allowCaptureBlockingPieceType = PieceType.Bishop;

    public IEnumerable<Move> Evaluate(ChessBoard board, AlgebraicPoint position, Piece movingPiece)
    {
        if (movingPiece.TimesMoved > 0)
            yield break;

        int rookXEnd = board.Width - 1;
        var castleRight = GetCastlingMovesInDirection(rookXEnd, 1, board, movingPiece, position);
        foreach (var move in castleRight)
            yield return move;

        int rookXStart = 0;
        var castleLeft = GetCastlingMovesInDirection(rookXStart, -1, board, movingPiece, position);
        foreach (var move in castleLeft)
            yield return move;
    }

    private IEnumerable<Move> GetCastlingMovesInDirection(
        int rookX,
        int directionX,
        ChessBoard board,
        Piece movingPiece,
        AlgebraicPoint position
    )
    {
        AlgebraicPoint rookPosition = new(rookX, position.Y);
        if (!board.TryGetPieceAt(rookPosition, out var rook) || rook.TimesMoved > 0)
            yield break;

        var targetPosition = new AlgebraicPoint(
            position.X + KingCastlingStepCount * directionX,
            position.Y
        );
        var targetRookPosition = new AlgebraicPoint(
            position.X + ((KingCastlingStepCount - 1) * directionX),
            position.Y
        );

        List<AlgebraicPoint> trigger = [];
        List<AlgebraicPoint> captures = [];
        for (int x = position.X + directionX; x != rookX; x += directionX)
        {
            AlgebraicPoint currentSquare = new(x, position.Y);
            var pieceOnSquare = board.PeekPieceAt(currentSquare);
            if (pieceOnSquare is null)
            {
                bool isAdjacentToKing = Math.Abs(currentSquare.X - position.X) == 1;
                if (currentSquare != targetPosition && !isAdjacentToKing)
                    trigger.Add(currentSquare);
                continue;
            }

            var isCapturedAllowed =
                pieceOnSquare.Type == _allowCaptureBlockingPieceType
                && pieceOnSquare.Color == movingPiece.Color;
            var isCaptureOnLandingSquare =
                currentSquare == targetPosition || currentSquare == targetRookPosition;
            if (!isCapturedAllowed || !isCaptureOnLandingSquare)
                yield break;

            captures.Add(currentSquare);
        }

        Move rookSideEffect = new(rookPosition, targetRookPosition, rook);
        yield return new Move(
            position,
            targetPosition,
            movingPiece,
            triggerSquares: trigger,
            capturedSquares: captures,
            sideEffects: [rookSideEffect]
        );
    }
}
