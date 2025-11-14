using System.ComponentModel;
using System.Text.Json.Serialization;
using AnarchyChess.Api.Game.Models;
using AnarchyChess.Api.Profile.DTOs;
using AnarchyChess.Api.Streaks.Entities;

namespace AnarchyChess.Api.Streaks.Models;

[DisplayName("WinStreak")]
[method: JsonConstructor]
public record WinStreakDto(MinimalProfile Profile, IReadOnlyList<GameToken> HighestStreakGameTokens)
{
    public WinStreakDto(UserWinStreak streak)
        : this(
            Profile: new MinimalProfile(streak.User),
            HighestStreakGameTokens: [.. streak.HighestStreakGames]
        ) { }
}
