using System.ComponentModel.DataAnnotations.Schema;
using Chess2.Api.Profile.Entities;
using Chess2.Api.Profile.Models;
using Microsoft.EntityFrameworkCore;

namespace Chess2.Api.Streaks.Entities;

[PrimaryKey(nameof(UserId))]
public class UserStreak
{
    public required UserId UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public AuthedUser User { get; set; } = null!;

    public int HighestStreak { get; set; }
    public int CurrentStreak { get; set; }
}
