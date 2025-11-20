using AnarchyChess.Api.GameLogic.Models;

namespace AnarchyChess.Api.GameSnapshot.Models;

public record IntermediateSquarePath(byte PositionIdx, bool IsCapture)
{
    public static IntermediateSquarePath FromIntermediateSquare(
        IntermediateSquare intermediateSquare,
        int boardWidth
    ) =>
        new(
            PositionIdx: intermediateSquare.Position.AsIndex(boardWidth),
            IsCapture: intermediateSquare.IsCapture
        );
}
