using System.ComponentModel;
using System.Text.Json.Serialization;
using Chess2.Api.Game.Models;
using Chess2.Api.Profile.DTOs;
using Chess2.Api.Streaks.Entities;

namespace Chess2.Api.Streaks.Models;

[DisplayName("WinStreak")]
[method: JsonConstructor]
public record WinStreakDto(
    MinimalProfile Profile,
    int HighestStreak,
    IReadOnlyCollection<GameToken> HighestStreakGames
)
{
    public WinStreakDto(UserWinStreak streak)
        : this(
            Profile: new MinimalProfile(streak.User),
            HighestStreak: streak.HighestStreak,
            HighestStreakGames: [.. streak.HighestStreakGames]
        ) { }
}
