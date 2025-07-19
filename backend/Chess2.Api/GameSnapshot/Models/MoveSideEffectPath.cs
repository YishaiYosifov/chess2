using Chess2.Api.GameLogic.Models;

namespace Chess2.Api.GameSnapshot.Models;

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
