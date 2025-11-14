using AnarchyChess.Api.GameLogic.Models;

namespace AnarchyChess.Api.ArchivedGames.Entities;

public class PieceSpawnArchive
{
    public int Id { get; set; }

    public required PieceType Type { get; set; }
    public required GameColor? Color { get; set; }
    public required byte PosIdx { get; set; }
}
