using System.ComponentModel.DataAnnotations.Schema;

namespace Chess2.Api.Game.Entities;

public class MoveArchive
{
    public int Id { get; set; }

    public required int MoveNumber { get; set; }
    public required string EncodedMove { get; set; }

    public int GameId { get; set; }

    [ForeignKey(nameof(GameId))]
    public GameArchive? Game { get; set; }
}
