using Chess2.Api.Profile.Entities;
using System.ComponentModel.DataAnnotations.Schema;

namespace Chess2.Api.Social.Entities;

public class StarredUser
{
    public int Id { get; set; }
    public required string UserId { get; set; }

    public required string StarredUserId { get; set; }

    [ForeignKey(nameof(StarredUserId))]
    public required AuthedUser Starred { get; set; }
}
