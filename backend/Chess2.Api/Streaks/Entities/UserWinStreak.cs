using System.ComponentModel.DataAnnotations.Schema;
using Chess2.Api.Profile.Entities;
using Chess2.Api.Profile.Models;
using Microsoft.EntityFrameworkCore;

namespace Chess2.Api.Streaks.Entities;

[PrimaryKey(nameof(UserId))]
public class UserWinStreak
{
    public required UserId UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public required AuthedUser User { get; set; }

    public IList<string> HighestStreakGames { get; set; } = [];
    public IList<string> CurrentStreakGames { get; set; } = [];
}
