using Chess2.Api.GameLogic.Models;

namespace Chess2.Api.GameSnapshot.Models;

[GenerateSerializer]
[Alias("Chess2.Api.GameSnapshot.Models.PieceSpawnPath")]
public record PieceSpawnPath(PieceType Type, GameColor? Color, byte PosIdx)
{
    public static PieceSpawnPath FromPieceSpawn(PieceSpawn pieceSpawn, int boardWidth) =>
        new(
            Type: pieceSpawn.Type,
            Color: pieceSpawn.Color,
            PosIdx: pieceSpawn.Position.AsIndex(boardWidth)
        );
}
