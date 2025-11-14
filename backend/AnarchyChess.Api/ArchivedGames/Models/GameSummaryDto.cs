using System.ComponentModel;
using AnarchyChess.Api.GameSnapshot.Models;
using AnarchyChess.Api.Game.Models;

namespace AnarchyChess.Api.ArchivedGames.Models;

[DisplayName("GameSummary")]
public record GameSummaryDto(
    GameToken GameToken,
    PlayerSummaryDto WhitePlayer,
    PlayerSummaryDto BlackPlayer,
    GameResult Result,
    DateTime CreatedAt
);
