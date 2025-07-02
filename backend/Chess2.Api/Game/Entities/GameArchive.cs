using System.ComponentModel.DataAnnotations.Schema;
using Chess2.Api.Game.Models;

namespace Chess2.Api.Game.Entities;

public class GameArchive
{
    public int Id { get; set; }
    public required string GameToken { get; set; }
    public required GameResult Result { get; set; }
    public required string ResultDescription { get; set; }
    public required string FinalFen { get; set; }
    public IEnumerable<MoveArchive> Moves { get; set; } = [];

    public int WhitePlayerId { get; set; }
    public int BlackPlayerId { get; set; }

    [ForeignKey(nameof(WhitePlayerId))]
    public required PlayerArchive? WhitePlayer { get; set; }

    [ForeignKey(nameof(BlackPlayerId))]
    public required PlayerArchive? BlackPlayer { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
