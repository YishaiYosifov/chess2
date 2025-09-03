using Chess2.Api.UserRating.Entities;
using System.ComponentModel;

namespace Chess2.Api.UserRating.Models;

[DisplayName("Rating")]
public record RatingSummary(int Rating, DateTime AchievedAt)
{
    public RatingSummary(RatingArchive rating)
        : this(Rating: rating.Value, AchievedAt: rating.AchievedAt) { }
}
