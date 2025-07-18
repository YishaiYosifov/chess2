using Chess2.Api.GameSnapshot.Models;

namespace Chess2.Api.UserRating.Entities;

public class CurrentRating
{
    public int Id { get; set; }
    public required string UserId { get; set; }

    public required TimeControl TimeControl { get; set; }
    public required int Value { get; set; }

    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}
