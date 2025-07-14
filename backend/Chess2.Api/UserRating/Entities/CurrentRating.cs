using Chess2.Api.Game.Models;
using Microsoft.EntityFrameworkCore;

namespace Chess2.Api.UserRating.Entities;

[PrimaryKey(nameof(UserId))]
public class CurrentRating
{
    public required string UserId { get; set; }

    public required TimeControl TimeControl { get; set; }
    public required int Value { get; set; }

    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}
