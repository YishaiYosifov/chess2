using AnarchyChess.Api.GameLogic.Models;

namespace AnarchyChess.Api.GameLogic.PieceMovementRules;

public class KnooklearFusionRule(PieceType fuseWith, params IPieceMovementRule[] rules)
    : IPieceMovementRule
{
    private readonly PieceType _fuseWith = fuseWith;

    private readonly Offset[] _explosionOffsets =
    [
        new Offset(X: 0, Y: 1),
        new Offset(X: 0, Y: -1),
        new Offset(X: 1, Y: 1),
        new Offset(X: 1, Y: 0),
        new Offset(X: 1, Y: -1),
        new Offset(X: -1, Y: 1),
        new Offset(X: -1, Y: 0),
        new Offset(X: -1, Y: -1),
    ];

    public IEnumerable<Move> Evaluate(
        IReadOnlyChessBoard board,
        AlgebraicPoint position,
        Piece movingPiece
    )
    {
        foreach (var rule in rules)
        {
            foreach (var move in rule.Evaluate(board, position, movingPiece))
            {
                if (
                    !move.Captures.Any(capture =>
                        capture.CapturedPiece.Type == _fuseWith
                        && capture.CapturedPiece.Color == movingPiece.Color
                    )
                )
                    yield return move;
                else
                    yield return BecomeDeathTheDestroyerOfWorlds(board, move);
            }
        }
    }

    private Move BecomeDeathTheDestroyerOfWorlds(IReadOnlyChessBoard board, Move move)
    {
        var position = move.To;
        List<MoveCapture> captures = [.. move.Captures];
        foreach (var offset in _explosionOffsets)
        {
            var target = position + offset;
            if (target == move.From)
                continue;

            var capturedPiece = board.PeekPieceAt(target);
            if (capturedPiece is null)
                continue;

            captures.Add(new MoveCapture(target, board));
        }

        return move with
        {
            Captures = captures,
            PromotesTo = PieceType.Knook,
            SpecialMoveType = SpecialMoveType.KnooklearFusion,
        };
    }
}
