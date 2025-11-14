using AnarchyChess.Api.GameLogic.Models;

namespace AnarchyChess.Api.GameSnapshot.Models;

[GenerateSerializer]
[Alias("AnarchyChess.Api.GameSnapshot.Models.PieceSpawnPath")]
public record PieceSpawnPath(PieceType Type, GameColor? Color, byte PosIdx)
{
    public static PieceSpawnPath FromPieceSpawn(PieceSpawn pieceSpawn, int boardWidth) =>
        new(
            Type: pieceSpawn.Type,
            Color: pieceSpawn.Color,
            PosIdx: pieceSpawn.Position.AsIndex(boardWidth)
        );
}
