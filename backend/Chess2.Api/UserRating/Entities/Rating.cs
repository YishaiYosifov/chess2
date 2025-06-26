using Chess2.Api.Game.Models;

namespace Chess2.Api.UserRating.Entities;

public class Rating
{
    public int Id { get; set; }
    public required string UserId { get; set; }

    public required TimeControl TimeControl { get; set; }

    public int Value { get; set; } = 800;

    public DateTime AchievedAt { get; set; } = DateTime.UtcNow;
}
