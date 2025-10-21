using System.ComponentModel;
using Chess2.Api.GameSnapshot.Models;
using Chess2.Api.Game.Models;

namespace Chess2.Api.ArchivedGames.Models;

[DisplayName("GameSummary")]
public record GameSummaryDto(
    GameToken GameToken,
    PlayerSummaryDto WhitePlayer,
    PlayerSummaryDto BlackPlayer,
    GameResult Result,
    DateTime CreatedAt
);
