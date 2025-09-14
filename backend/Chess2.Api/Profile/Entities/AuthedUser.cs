using Microsoft.AspNetCore.Identity;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Chess2.Api.Profile.Entities;

public class AuthedUser : IdentityUser
{
    [MaxLength(500)]
    public required string About { get; set; }

    [MaxLength(6)]
    [DefaultValue("XX")]
    public required string CountryCode { get; set; }

    public DateTime? UsernameLastChanged { get; set; }
    public int QuestPoints { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
