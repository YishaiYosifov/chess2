using System.ComponentModel;

namespace Chess2.Api.Game.Models;

[DisplayName("PlayerSummary")]
public record PlayerSummaryDto(string UserId, string UserName, int? Rating);
