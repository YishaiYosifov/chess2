using System.ComponentModel.DataAnnotations.Schema;
using AnarchyChess.Api.Profile.Entities;
using AnarchyChess.Api.Profile.Models;

namespace AnarchyChess.Api.Auth.Entities;

public class RefreshToken
{
    public int Id { get; set; }
    public required UserId UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public required AuthedUser User { get; set; }

    public required string Jti { get; set; }
    public bool IsRevoked { get; set; }
    public required DateTimeOffset ExpiresAt { get; set; }
}
