using AnarchyChess.Api.GameLogic.Models;

namespace AnarchyChess.Api.GameSnapshot.Models;

[GenerateSerializer]
[Alias("AnarchyChess.Api.GameSnapshot.Models.IntermediateSquarePath")]
public record IntermediateSquarePath(byte PosIdx, bool IsCapture)
{
    public static IntermediateSquarePath FromIntermediateSquare(
        IntermediateSquare intermediateSquare,
        int boardWidth
    ) =>
        new(
            PosIdx: intermediateSquare.Position.AsIndex(boardWidth),
            IsCapture: intermediateSquare.IsCapture
        );
}
