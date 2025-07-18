namespace Chess2.Api.GameSnapshot.Models;

public record GameResultData(
    GameResult Result,
    string ResultDescription,
    int? WhiteRatingChange,
    int? BlackRatingChange
);
