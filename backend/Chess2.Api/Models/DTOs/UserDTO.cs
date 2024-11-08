using Chess2.Api.Models.Entities;

namespace Chess2.Api.Models.DTOs;

public class UserIn
{
    public required string Username { get; set; }
    public required string Email { get; set; }
    public required string Password { get; set; }
}

public class UserLogin
{
    public required string UsernameOrEmail { get; set; }
    public required string Password { get; set; }
}

public class UserOut
{
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    public UserOut() { }

    public UserOut(User user)
    {
        UserId = user.UserId;
        Username = user.Username;
        Email = user.Email;
    }
}
