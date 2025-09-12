using Chess2.Api.GameLogic.Models;

namespace Chess2.Api.ArchivedGames.Entities;

public class PlayerArchive
{
    public int Id { get; set; }
    public required string UserId { get; set; }

    public required bool IsAuthenticated { get; set; }
    public required GameColor Color { get; set; }
    public required string UserName { get; set; }
    public required double FinalTimeRemaining { get; set; }
    public required int? NewRating { get; set; }
    public required int? RatingChange { get; set; }
    public required string CountryCode { get; set; }
}
