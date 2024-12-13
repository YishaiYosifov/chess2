namespace Chess2.Api.Models;

public class User
{
    public required string UserId { get; set; }
    public required bool IsGuest { get; set; }
}
