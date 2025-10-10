using System.ComponentModel.DataAnnotations.Schema;
using Chess2.Api.Profile.Entities;
using Chess2.Api.Profile.Models;

namespace Chess2.Api.Auth.Entities;

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
