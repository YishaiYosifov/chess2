using System.ComponentModel.DataAnnotations.Schema;
using Chess2.Api.Profile.Entities;
using Chess2.Api.Profile.Models;
using Microsoft.EntityFrameworkCore;

namespace Chess2.Api.Social.Entities;

[Index(nameof(UserId), nameof(StarredUserId), IsUnique = true)]
public class StarredUser
{
    public int Id { get; set; }
    public required UserId UserId { get; set; }

    public required UserId StarredUserId { get; set; }

    [ForeignKey(nameof(StarredUserId))]
    public required AuthedUser Starred { get; set; }
}
