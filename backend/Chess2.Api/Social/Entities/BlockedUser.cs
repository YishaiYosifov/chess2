using System.ComponentModel.DataAnnotations.Schema;
using Chess2.Api.Profile.Entities;
using Microsoft.EntityFrameworkCore;

namespace Chess2.Api.Social.Entities;

[Index(nameof(UserId), nameof(BlockedUserId), IsUnique = true)]
public class BlockedUser
{
    public int Id { get; set; }
    public required string UserId { get; set; }

    public required string BlockedUserId { get; set; }

    [ForeignKey(nameof(BlockedUserId))]
    public required AuthedUser Blocked { get; set; }
}
