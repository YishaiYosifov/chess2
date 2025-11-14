using AnarchyChess.Api.GameLogic.Extensions;
using AnarchyChess.Api.GameLogic.Models;

namespace AnarchyChess.Api.GameLogic.PieceMovementRules;

public class CastleRule : IPieceMovementRule
{
    private const int KingDestStep = 2;
    private const int RookDestStep = 1;

    private readonly HashSet<PieceType> _allowCaptureBlockingPieceType = [PieceType.Bishop];

    public IEnumerable<Move> Evaluate(
        IReadOnlyChessBoard board,
        AlgebraicPoint position,
        Piece movingPiece
    )
    {
        if (movingPiece.TimesMoved > 0)
            yield break;

        foreach (var move in GetKingSide(board, position, movingPiece))
            yield return move;

        foreach (var move in GetQueenSide(board, position, movingPiece))
            yield return move;

        foreach (var move in GetVertical(board, position, movingPiece))
            yield return move;
    }

    private IEnumerable<Move> GetKingSide(
        IReadOnlyChessBoard board,
        AlgebraicPoint position,
        Piece movingPiece
    ) =>
        GetCastlingMovesInDirection(
            rookPosition: new AlgebraicPoint(board.Width - 1, position.Y),
            board,
            movingPiece,
            position,
            SpecialMoveType.KingsideCastle
        );

    private IEnumerable<Move> GetQueenSide(
        IReadOnlyChessBoard board,
        AlgebraicPoint position,
        Piece movingPiece
    ) =>
        GetCastlingMovesInDirection(
            rookPosition: new AlgebraicPoint(0, position.Y),
            board,
            movingPiece,
            position,
            SpecialMoveType.QueensideCastle
        );

    private IEnumerable<Move> GetVertical(
        IReadOnlyChessBoard board,
        AlgebraicPoint position,
        Piece movingPiece
    )
    {
        if (movingPiece.Color is null)
            return [];

        var rookY = movingPiece.Color.Value.Match(whenWhite: board.Height - 1, whenBlack: 0);
        return GetCastlingMovesInDirection(
            rookPosition: new AlgebraicPoint(position.X, rookY),
            board,
            movingPiece,
            position,
            SpecialMoveType.VerticalCastle
        );
    }

    private IEnumerable<Move> GetCastlingMovesInDirection(
        AlgebraicPoint rookPosition,
        IReadOnlyChessBoard board,
        Piece movingPiece,
        AlgebraicPoint position,
        SpecialMoveType moveType
    )
    {
        if (
            !board.TryGetPieceAt(rookPosition, out var rook)
            || rook.TimesMoved > 0
            || rook.Color != movingPiece.Color
            || rook.Type != PieceType.Rook
        )
            yield break;

        AlgebraicPoint? targetPosition = null;
        AlgebraicPoint? targetRookPosition = null;

        List<AlgebraicPoint> trigger = [];
        List<MoveCapture> captures = [];

        int dx = rookPosition.X - position.X;
        int dy = rookPosition.Y - position.Y;
        int steps = Math.Max(Math.Abs(dx), Math.Abs(dy));
        for (int step = 1; step < steps; step++)
        {
            AlgebraicPoint currentSquare = new(
                position.X + step * dx / steps,
                position.Y + step * dy / steps
            );
            if (!board.IsWithinBoundaries(currentSquare))
                yield break;

            if (step == RookDestStep)
                targetRookPosition = currentSquare;
            else if (step == KingDestStep)
                targetPosition = currentSquare;

            var pieceOnSquare = board.PeekPieceAt(currentSquare);
            if (pieceOnSquare is null)
            {
                // we don't want to add the target position if it is the current square
                // because target position is already a trigger
                if (step != 1 && step != KingDestStep)
                    trigger.Add(currentSquare);
                continue;
            }

            bool isCapturedAllowed =
                _allowCaptureBlockingPieceType.Contains(pieceOnSquare.Type)
                && pieceOnSquare.Color == movingPiece.Color;
            bool isCaptureOnLandingSquare = step == KingDestStep || step == RookDestStep;
            if (!isCapturedAllowed || !isCaptureOnLandingSquare)
                yield break;

            captures.Add(new MoveCapture(currentSquare, board));
        }

        if (targetPosition is null || targetRookPosition is null)
            yield break;

        MoveSideEffect rookSideEffect = new(
            From: rookPosition,
            To: targetRookPosition.Value,
            Piece: rook
        );
        yield return new Move(
            position,
            targetPosition.Value,
            movingPiece,
            triggerSquares: trigger,
            captures: captures,
            sideEffects: [rookSideEffect],
            specialMoveType: moveType
        );
    }
}
