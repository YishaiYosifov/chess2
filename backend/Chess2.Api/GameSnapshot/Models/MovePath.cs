using Chess2.Api.GameLogic.Models;

namespace Chess2.Api.GameSnapshot.Models;

[GenerateSerializer]
[Alias("Chess2.Api.GameSnapshot.Models.MovePath")]
public record MovePath(
    byte FromIdx,
    byte ToIdx,
    IReadOnlyCollection<byte>? CapturedIdxs,
    IReadOnlyCollection<byte>? TriggerIdxs,
    IReadOnlyCollection<byte>? IntermediateIdxs,
    IReadOnlyList<MoveSideEffectPath>? SideEffects,
    PieceType? PromotesTo
)
{
    public static MovePath FromMove(Move move, int boardWidth)
    {
        var captures =
            move.Captures.Count != 0
                ? move.Captures.Select(c => c.Position.AsIndex(boardWidth)).ToList()
                : null;
        var triggers =
            move.TriggerSquares.Count != 0
                ? move.TriggerSquares.Select(t => t.AsIndex(boardWidth)).ToList()
                : null;
        var intermediates =
            move.IntermediateSquares.Count != 0
                ? move.IntermediateSquares.Select(i => i.AsIndex(boardWidth)).ToList()
                : null;
        var sideEffects =
            move.SideEffects.Count != 0
                ? move
                    .SideEffects.Select(m => MoveSideEffectPath.FromMoveSideEffect(m, boardWidth))
                    .ToList()
                : null;

        return new(
            FromIdx: move.From.AsIndex(boardWidth),
            ToIdx: move.To.AsIndex(boardWidth),
            CapturedIdxs: captures,
            TriggerIdxs: triggers,
            IntermediateIdxs: intermediates,
            SideEffects: sideEffects,
            PromotesTo: move.PromotesTo
        );
    }
}
