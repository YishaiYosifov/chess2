using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Chess2.Api.Profile.Models;
using Microsoft.AspNetCore.Identity;

namespace Chess2.Api.Profile.Entities;

public class AuthedUser : IdentityUser<UserId>
{
    [MaxLength(500)]
    public required string About { get; set; }

    [MaxLength(6)]
    [DefaultValue("XX")]
    public required string CountryCode { get; set; }

    public DateTime? UsernameLastChanged { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
