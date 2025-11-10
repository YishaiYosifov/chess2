using Chess2.Api.Game.Models;
using Chess2.Api.Profile.DTOs;
using Chess2.Api.Streaks.Entities;
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace Chess2.Api.Streaks.Models;

[DisplayName("Streak")]
[method: JsonConstructor]
public record StreakDto(
    MinimalProfile Profile,
    int HighestStreak,
    IReadOnlyCollection<GameToken> HighestStreakGames
)
{
    public StreakDto(UserStreak streak)
        : this(
            Profile: new MinimalProfile(streak.User),
            HighestStreak: streak.HighestStreak,
            HighestStreakGames: [.. streak.HighestStreakGames]
        ) { }
}
