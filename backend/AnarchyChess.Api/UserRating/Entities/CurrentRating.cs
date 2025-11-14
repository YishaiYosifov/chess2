using AnarchyChess.Api.GameSnapshot.Models;
using AnarchyChess.Api.Profile.Models;

namespace AnarchyChess.Api.UserRating.Entities;

public class CurrentRating
{
    public int Id { get; set; }
    public required UserId UserId { get; set; }

    public required TimeControl TimeControl { get; set; }
    public required int Value { get; set; }

    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}
