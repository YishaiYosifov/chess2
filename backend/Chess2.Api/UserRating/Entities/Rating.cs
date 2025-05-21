using Chess2.Api.Game.Models;
using Chess2.Api.Users.Entities;
using System.ComponentModel.DataAnnotations.Schema;

namespace Chess2.Api.UserRating.Entities;

public class Rating
{
    public int Id { get; set; }
    public required string UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public required AuthedUser User { get; set; }

    public required TimeControl TimeControl { get; set; }

    public int Value { get; set; } = 800;
}
