namespace Chess2.Api.GameSnapshot.Models;

[GenerateSerializer]
[Alias("Chess2.Api.GameSnapshot.Models.GameResultData")]
public record GameResultData(
    GameResult Result,
    string ResultDescription,
    int? WhiteRatingChange,
    int? BlackRatingChange
);
