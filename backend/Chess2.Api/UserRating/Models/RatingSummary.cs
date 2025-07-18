using System.ComponentModel;
using Chess2.Api.UserRating.Entities;

namespace Chess2.Api.UserRating.Models;

[DisplayName("Rating")]
public record RatingSummary(int Rating, long At)
{
    public RatingSummary(RatingArchive rating)
        : this(
            Rating: rating.Value,
            At: new DateTimeOffset(rating.AchievedAt).ToUnixTimeMilliseconds()
        ) { }
}
