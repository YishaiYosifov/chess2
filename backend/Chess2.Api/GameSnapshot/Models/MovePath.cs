using Chess2.Api.GameLogic.Models;

namespace Chess2.Api.GameSnapshot.Models;

public record MovePath(
    byte FromIdx,
    byte ToIdx,
    IReadOnlyList<byte>? CapturedIdxs,
    IReadOnlyList<byte>? TriggerIdxs,
    IReadOnlyList<MoveSideEffectPath>? SideEffects,
    PieceType? PromotesTo
)
{
    public static MovePath FromMove(Move move, int boardWidth)
    {
        var captures = move.CapturedSquares.Any()
            ? move.CapturedSquares.Select(c => c.AsIndex(boardWidth)).ToList()
            : null;
        var triggers = move.TriggerSquares.Any()
            ? move.TriggerSquares.Select(t => t.AsIndex(boardWidth)).ToList()
            : null;
        var sideEffects = move.SideEffects.Any()
            ? move
                .SideEffects.Select(m => MoveSideEffectPath.FromMoveSideEffect(m, boardWidth))
                .ToList()
            : null;

        return new(
            FromIdx: move.From.AsIndex(boardWidth),
            ToIdx: move.To.AsIndex(boardWidth),
            CapturedIdxs: captures,
            TriggerIdxs: triggers,
            SideEffects: sideEffects,
            PromotesTo: move.PromotesTo
        );
    }
}
