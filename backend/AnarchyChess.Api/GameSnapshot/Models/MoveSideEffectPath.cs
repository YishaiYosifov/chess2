using AnarchyChess.Api.GameLogic.Models;

namespace AnarchyChess.Api.GameSnapshot.Models;

[GenerateSerializer]
[Alias("AnarchyChess.Api.GameSnapshot.Models.MoveSideEffectPath")]
public record MoveSideEffectPath(byte FromIdx, byte ToIdx)
{
    public static MoveSideEffectPath FromMoveSideEffect(
        MoveSideEffect moveSideEffect,
        int boardWidth
    ) =>
        new(
            FromIdx: moveSideEffect.From.AsIndex(boardWidth),
            ToIdx: moveSideEffect.To.AsIndex(boardWidth)
        );
}
