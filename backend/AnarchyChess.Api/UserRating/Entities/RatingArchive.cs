using AnarchyChess.Api.GameSnapshot.Models;
using AnarchyChess.Api.Profile.Models;
using Microsoft.EntityFrameworkCore;

namespace AnarchyChess.Api.UserRating.Entities;

[Index(nameof(UserId))]
public class RatingArchive
{
    public int Id { get; set; }
    public required UserId UserId { get; set; }

    public required TimeControl TimeControl { get; set; }
    public required int Value { get; set; }

    public DateTime AchievedAt { get; set; } = DateTime.UtcNow;
}
