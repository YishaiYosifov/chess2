using System.ComponentModel;

namespace Chess2.Api.ArchivedGames.Models;

[DisplayName("PlayerSummary")]
public record PlayerSummaryDto(string UserId, bool IsAuthenticated, string UserName, int? Rating);
