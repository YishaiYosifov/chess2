using System.ComponentModel;
using Chess2.Api.Profile.Models;

namespace Chess2.Api.ArchivedGames.Models;

[DisplayName("PlayerSummary")]
public record PlayerSummaryDto(UserId UserId, string UserName, int? Rating);
