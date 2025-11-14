using System.ComponentModel;
using AnarchyChess.Api.Profile.Models;

namespace AnarchyChess.Api.ArchivedGames.Models;

[DisplayName("PlayerSummary")]
public record PlayerSummaryDto(UserId UserId, string UserName, int? Rating);
