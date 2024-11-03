namespace Chess2.Api.Models.Requests;

public class UserIn
{
    public required string Username { get; set; }
    public required string Email { get; set; }

    public required string Password { get; set; }
}
