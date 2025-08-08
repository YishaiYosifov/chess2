using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Chess2.Api.Users.Entities;

public class AuthedUser : IdentityUser
{
    [MaxLength(300)]
    public string? About { get; set; }

    [MaxLength(2)]
    public string? CountryCode { get; set; }

    public DateTime UsernameLastChanged { get; set; } = DateTime.UtcNow;
    public DateTime PasswordLastChanged { get; set; } = DateTime.UtcNow;
}
