using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Chess2.Api.Models.Entities;

public class AuthedUser : IdentityUser<int>
{
    [MaxLength(30)]
    public string Username { get; set; } = string.Empty;

    [MaxLength(300)]
    public string About { get; set; } = string.Empty;

    [MaxLength(2)]
    public string? CountryCode { get; set; }

    public byte[] PasswordSalt { get; set; } = [];

    public DateTime UsernameLastChanged { get; set; } = DateTime.UtcNow;
    public DateTime PasswordLastChanged { get; set; } = DateTime.UtcNow;

    public ICollection<Rating> Ratings { get; set; } = [];
}
