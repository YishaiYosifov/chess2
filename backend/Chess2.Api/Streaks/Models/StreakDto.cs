using System.ComponentModel;
using Chess2.Api.Profile.DTOs;

namespace Chess2.Api.Streaks.Models;

[DisplayName("Streak")]
public record StreakDto(MinimalProfile Profile, int HighestStreak);
