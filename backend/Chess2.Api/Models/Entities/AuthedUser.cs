using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Chess2.Api.Models.Entities;

public class AuthedUser : IdentityUser<int>
{
    [MaxLength(300)]
    public string? About { get; set; }

    [MaxLength(2)]
    public string? CountryCode { get; set; }

    public DateTime UsernameLastChanged { get; set; } = DateTime.UtcNow;
    public DateTime PasswordLastChanged { get; set; } = DateTime.UtcNow;

    public ICollection<Rating> Ratings { get; set; } = [];
}
