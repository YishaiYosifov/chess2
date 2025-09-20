using Chess2.Api.GameLogic.Models;

namespace Chess2.Api.GameLogic.PieceMovementRules;

public class CheckerCaptureRule(params Offset[] offsets) : IPieceMovementRule
{
    private readonly Offset[] _offsets = offsets;

    public IEnumerable<Move> Evaluate(ChessBoard board, AlgebraicPoint position, Piece movingPiece)
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
        ChessBoard board,
        AlgebraicPoint origin,
        AlgebraicPoint currentPosition,
        Piece movingPiece,
        HashSet<AlgebraicPoint> visited,
        HashSet<AlgebraicPoint> intermediates,
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

        if (encounteredPiece.Color != movingPiece.Color)
            captured.Add(new MoveCapture(currentPosition, board));
        var capturePosition = currentPosition;

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
            intermediateSquares: intermediates,
            triggerSquares: [capturePosition]
        );

        foreach (var offset in _offsets)
        foreach (
            var move in FindCaptureSequences(
                board,
                origin,
                currentPosition,
                movingPiece,
                visited: [.. visited],
                intermediates: [.. intermediates, currentPosition],
                captured: [.. captured],
                offset
            )
        )
            yield return move;
    }
}
