using System.ComponentModel.DataAnnotations.Schema;

namespace Chess2.Api.Models.Entities;

public class UserEntity
{
    public int UserId { get; set; }
    public required string Username { get; set; }
    public required string Email { get; set; }

    public required byte[] PasswordHash { get; set; }
    public required byte[] PasswordSalt { get; set; }
}
