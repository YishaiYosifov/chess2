using System.ComponentModel;

namespace Chess2.Api.UserRating.Models;

[DisplayName("Rating")]
public record RatingDto(int Rating, double At);
