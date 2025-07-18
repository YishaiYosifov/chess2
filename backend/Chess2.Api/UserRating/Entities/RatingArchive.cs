using Chess2.Api.GameSnapshot.Models;
using Microsoft.EntityFrameworkCore;

namespace Chess2.Api.UserRating.Entities;

[Index(nameof(UserId))]
public class RatingArchive
{
    public int Id { get; set; }
    public required string UserId { get; set; }

    public required TimeControl TimeControl { get; set; }
    public required int Value { get; set; }

    public DateTime AchievedAt { get; set; } = DateTime.UtcNow;
}
