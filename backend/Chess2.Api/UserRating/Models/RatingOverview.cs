using Chess2.Api.Game.Models;

namespace Chess2.Api.UserRating.Models;

public record RatingOverview(
    TimeControl TimeControl,
    IEnumerable<RatingSummary> Ratings,
    int Current,
    int Highest,
    int Lowest
);
