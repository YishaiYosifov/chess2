using Chess2.Api.Models.Entities;

namespace Chess2.Api.Models.DTOs;

public class UserIn
{
    public required string Username { get; set; }
    public required string Email { get; set; }
    public required string Password { get; set; }
}

public class UserOut(User user)
{
    public int UserId { get; set; } = user.UserId;
    public string Username { get; set; } = user.Username;
    public string Email { get; set; } = user.Email;
}