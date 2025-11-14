using System.ComponentModel.DataAnnotations.Schema;
using AnarchyChess.Api.Profile.Entities;
using AnarchyChess.Api.Profile.Models;
using Microsoft.EntityFrameworkCore;

namespace AnarchyChess.Api.Social.Entities;

[Index(nameof(UserId), nameof(BlockedUserId), IsUnique = true)]
public class BlockedUser
{
    public int Id { get; set; }
    public required UserId UserId { get; set; }

    public required UserId BlockedUserId { get; set; }

    [ForeignKey(nameof(BlockedUserId))]
    public required AuthedUser Blocked { get; set; }
}
