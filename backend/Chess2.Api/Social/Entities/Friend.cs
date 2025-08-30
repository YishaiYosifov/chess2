using System.ComponentModel.DataAnnotations.Schema;
using Chess2.Api.Profile.Entities;

namespace Chess2.Api.Social.Entities;

public class Friend
{
    public int Id { get; set; }

    public required string UserId1 { get; set; }

    [ForeignKey(nameof(UserId1))]
    public required AuthedUser User1 { get; set; }

    public required string UserId2 { get; set; }

    [ForeignKey(nameof(UserId2))]
    public required AuthedUser User2 { get; set; }
}
