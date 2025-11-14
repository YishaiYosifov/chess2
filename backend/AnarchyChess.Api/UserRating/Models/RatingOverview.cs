using AnarchyChess.Api.GameSnapshot.Models;

namespace AnarchyChess.Api.UserRating.Models;

public record RatingOverview(
    TimeControl TimeControl,
    IEnumerable<RatingSummary> Ratings,
    int Current,
    int Highest,
    int Lowest
);
