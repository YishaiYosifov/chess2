using Chess2.Api.GameLogic.Models;

namespace Chess2.Api.Game.Entities;

public class PlayerArchive
{
    public int Id { get; set; }
    public required string UserId { get; set; }
    public required GameColor Color { get; set; }
}
