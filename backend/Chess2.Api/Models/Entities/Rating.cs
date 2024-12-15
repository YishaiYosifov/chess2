using System.ComponentModel.DataAnnotations.Schema;

namespace Chess2.Api.Models.Entities;

public class Rating
{
    public int RatingId { get; set; }
    public int UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public required AuthedUser User { get; set; }

    public int Value { get; set; }
}
