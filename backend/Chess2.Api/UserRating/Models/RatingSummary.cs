using System.ComponentModel;
using System.Text.Json.Serialization;
using Chess2.Api.UserRating.Entities;

namespace Chess2.Api.UserRating.Models;

[DisplayName("Rating")]
[method: JsonConstructor]
public record RatingSummary(int Rating, DateTime AchievedAt)
{
    public RatingSummary(RatingArchive rating)
        : this(Rating: rating.Value, AchievedAt: rating.AchievedAt) { }
}
