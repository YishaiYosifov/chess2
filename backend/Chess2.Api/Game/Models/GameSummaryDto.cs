using System.ComponentModel;

namespace Chess2.Api.Game.Models;

[DisplayName("GameSummary")]
public record GameSummaryDto(
    string GameToken,
    PlayerSummaryDto WhitePlayer,
    PlayerSummaryDto BlackPlayer,
    GameResult Result,
    DateTime CreatedAt
);
