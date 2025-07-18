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

        int rookXKingside = board.Width - 1;
        var castleKingside = GetCastlingMovesInDirection(
            rookXKingside,
            directionX: 1,
            board,
            movingPiece,
            position,
            SpecialMoveType.KingsideCastle
        );
        foreach (var move in castleKingside)
            yield return move;

        int rookXQueenside = 0;
        var castleQueenside = GetCastlingMovesInDirection(
            rookXQueenside,
            directionX: -1,
            board,
            movingPiece,
            position,
            SpecialMoveType.QueensideCastle
        );
        foreach (var move in castleQueenside)
            yield return move;
    }

    private IEnumerable<Move> GetCastlingMovesInDirection(
        int rookX,
        int directionX,
        ChessBoard board,
        Piece movingPiece,
        AlgebraicPoint position,
        SpecialMoveType moveType
    )
    {
        AlgebraicPoint rookPosition = new(rookX, position.Y);
        if (
            !board.TryGetPieceAt(rookPosition, out var rook)
            || rook.TimesMoved > 0
            || rook.Color != movingPiece.Color
            || rook.Type != PieceType.Rook
        )
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
            if (!board.IsWithinBoundaries(currentSquare))
                break;

            var pieceOnSquare = board.PeekPieceAt(currentSquare);
            if (pieceOnSquare is null)
            {
                bool isAdjacentToKing = Math.Abs(currentSquare.X - position.X) == 1;
                // we don't want to add the target position if it is the current square
                // because target position is already a trigger
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
            sideEffects: [rookSideEffect],
            specialMoveType: moveType
        );
    }
}
