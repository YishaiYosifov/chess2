using System.ComponentModel.DataAnnotations;

namespace Chess2.Api.Models.Entities;

public class User
{
    public int UserId { get; set; }

    [MaxLength(30)]
    public required string Username { get; set; }
    [MaxLength(256)]
    public required string Email { get; set; }

    [MaxLength(300)]
    public string About { get; set; } = string.Empty;
    [MaxLength(2)]
    public string? CountryCode { get; set; }

    public required byte[] PasswordHash { get; set; }
    public required byte[] PasswordSalt { get; set; }

    public DateTime PasswordLastChanged { get; set; } = DateTime.UtcNow;
}
