using Chess2.Api.GameLogic.Models;

namespace Chess2.Api.GameLogic.PieceMovementRules;

public class IlVaticanoRule(Offset stepOffset, int partnerDistance = 3) : IPieceMovementRule
{
    private readonly Offset _stepOffset = stepOffset;
    private readonly int _partnerDistance = partnerDistance;

    public IEnumerable<Move> Evaluate(ChessBoard board, AlgebraicPoint position, Piece movingPiece)
    {
        var target = position + (_stepOffset * _partnerDistance);
        if (
            !board.IsWithinBoundaries(target)
            || !board.TryGetPieceAt(target, out var partnerPiece)
            || partnerPiece.Type != movingPiece.Type
            || partnerPiece.Color != movingPiece.Color
        )
            yield break;

        List<MoveCapture> captures = [];
        List<AlgebraicPoint> triggers = [];
        var stepPoint = position;
        for (var i = 0; i < _partnerDistance - 1; i++)
        {
            stepPoint += _stepOffset;
            if (
                !board.IsWithinBoundaries(stepPoint)
                || !board.TryGetPieceAt(stepPoint, out var capturePiece)
                || capturePiece.Type is not (PieceType.Pawn or PieceType.UnderagePawn)
                || capturePiece.Color == movingPiece.Color
            )
                yield break;

            captures.Add(new MoveCapture(stepPoint, board));
            triggers.Add(stepPoint);
        }

        // move the piece we're performing il vaticano with
        // to the square right after our starting position
        MoveSideEffect sideEffect = new(From: target, To: position + _stepOffset, partnerPiece);

        yield return new Move(
            from: position,
            to: stepPoint,
            piece: movingPiece,
            triggerSquares: triggers,
            captures: captures,
            sideEffects: [sideEffect],
            specialMoveType: SpecialMoveType.IlVaticano
        );
    }
}
