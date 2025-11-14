using System.ComponentModel;
using System.Text.Json.Serialization;
using AnarchyChess.Api.UserRating.Entities;

namespace AnarchyChess.Api.UserRating.Models;

[DisplayName("Rating")]
[method: JsonConstructor]
public record RatingSummary(int Rating, DateTime AchievedAt)
{
    public RatingSummary(RatingArchive rating)
        : this(Rating: rating.Value, AchievedAt: rating.AchievedAt) { }
}
