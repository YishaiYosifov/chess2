using AnarchyChess.Api.GameSnapshot.Models;

namespace AnarchyChess.Api.UserRating.Models;

public record CurrentRatingStatus(TimeControl TimeControl, int Rating);
