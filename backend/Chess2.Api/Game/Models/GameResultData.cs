using Chess2.Api.UserRating.Models;

namespace Chess2.Api.Game.Models;

public record GameResultData(
    GameResult Result,
    string ResultDescription,
    int? WhiteRatingChange,
    int? BlackRatingChange
);
