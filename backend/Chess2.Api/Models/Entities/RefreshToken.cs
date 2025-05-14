using System.ComponentModel.DataAnnotations.Schema;

namespace Chess2.Api.Models.Entities;

public class RefreshToken
{
    public int Id { get; set; }
    public int UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public required AuthedUser User { get; set; }

    public required string Token { get; set; }
    public bool IsInvalidated { get; set; }
    public required DateTime ExpiresAt { get; set; }
}
