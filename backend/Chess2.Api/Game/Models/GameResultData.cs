namespace Chess2.Api.Game.Models;

public record GameResultData(
    GameResult Result,
    string ResultDescription,
    int? WhiteRatingDelta,
    int? BlackRatingDelta
);
