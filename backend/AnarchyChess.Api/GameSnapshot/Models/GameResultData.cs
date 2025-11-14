namespace AnarchyChess.Api.GameSnapshot.Models;

[GenerateSerializer]
[Alias("AnarchyChess.Api.GameSnapshot.Models.GameResultData")]
public record GameResultData(
    GameResult Result,
    string ResultDescription,
    int? WhiteRatingChange,
    int? BlackRatingChange
);
