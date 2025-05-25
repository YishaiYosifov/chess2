using Chess2.Api.UserRating.Entities;
using Chess2.Api.Users.Models;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Chess2.Api.Users.Entities;

public class AuthedUser : IdentityUser, IUser
{

    [MaxLength(300)]
    public string? About { get; set; }

    [MaxLength(2)]
    public string? CountryCode { get; set; }

    public DateTime UsernameLastChanged { get; set; } = DateTime.UtcNow;
    public DateTime PasswordLastChanged { get; set; } = DateTime.UtcNow;

    public ICollection<Rating> Ratings { get; set; } = [];
}
