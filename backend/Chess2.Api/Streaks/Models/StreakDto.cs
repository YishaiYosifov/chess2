using Chess2.Api.Game.Models;
using Chess2.Api.Profile.DTOs;
using System.ComponentModel;

namespace Chess2.Api.Streaks.Models;

[DisplayName("Streak")]
public record StreakDto(
    MinimalProfile Profile,
    int HighestStreak,
    IReadOnlyCollection<GameToken> HighestStreakGames
);
