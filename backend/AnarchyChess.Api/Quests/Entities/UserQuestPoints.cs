using System.ComponentModel.DataAnnotations.Schema;
using AnarchyChess.Api.Profile.Entities;
using AnarchyChess.Api.Profile.Models;
using Microsoft.EntityFrameworkCore;

namespace AnarchyChess.Api.Quests.Entities;

[PrimaryKey(nameof(UserId))]
public class UserQuestPoints
{
    public required UserId UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public required AuthedUser User { get; set; }

    public int Points { get; set; }
}
