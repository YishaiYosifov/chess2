using System.ComponentModel;
using Chess2.Api.GameSnapshot.Models;

namespace Chess2.Api.ArchivedGames.Models;

[DisplayName("GameSummary")]
public record GameSummaryDto(
    string GameToken,
    PlayerSummaryDto WhitePlayer,
    PlayerSummaryDto BlackPlayer,
    GameResult Result,
    DateTime CreatedAt
);
