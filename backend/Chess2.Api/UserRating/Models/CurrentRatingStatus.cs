using Chess2.Api.GameSnapshot.Models;

namespace Chess2.Api.UserRating.Models;

public record CurrentRatingStatus(TimeControl TimeControl, int Rating);
