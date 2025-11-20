using AnarchyChess.Api.GameLogic.Models;

namespace AnarchyChess.Api.GameLogic.PieceMovementRules;

public class CheckerJumpRule(params Offset[] offsets) : IPieceMovementRule
{
    private readonly Offset[] _offsets = offsets;

    public IEnumerable<Move> Evaluate(
        IReadOnlyChessBoard board,
        AlgebraicPoint position,
        Piece movingPiece
    )
    {
        foreach (var offset in _offsets)
        foreach (
            var move in FindCaptureSequences(
                board,
                origin: position,
                currentPosition: position,
                movingPiece,
                visited: [],
                intermediates: [],
                captured: [],
                currentOffset: offset
            )
        )
            yield return move;
    }

    private IEnumerable<Move> FindCaptureSequences(
        IReadOnlyChessBoard board,
        AlgebraicPoint origin,
        AlgebraicPoint currentPosition,
        Piece movingPiece,
        HashSet<AlgebraicPoint> visited,
        HashSet<IntermediateSquare> intermediates,
        HashSet<MoveCapture> captured,
        Offset currentOffset
    )
    {
        currentPosition += currentOffset;
        if (visited.Contains(currentPosition))
            yield break;

        visited.Add(currentPosition);
        if (!board.TryGetPieceAt(currentPosition, out var encounteredPiece))
            yield break;

        var isCapture = encounteredPiece.Color != movingPiece.Color;
        if (isCapture)
            captured.Add(new MoveCapture(currentPosition, board));

        currentPosition += currentOffset;
        if (
            !board.IsWithinBoundaries(currentPosition)
            || board.PeekPieceAt(currentPosition) is not null
        )
            yield break;

        yield return new Move(
            origin,
            currentPosition,
            movingPiece,
            captures: captured,
            intermediateSquares: intermediates
        );

        IntermediateSquare intermediate = new(currentPosition, IsCapture: isCapture);
        foreach (var offset in _offsets)
        foreach (
            var move in FindCaptureSequences(
                board,
                origin,
                currentPosition,
                movingPiece,
                visited: [.. visited],
                intermediates: [.. intermediates, intermediate],
                captured: [.. captured],
                offset
            )
        )
            yield return move;
    }
}
