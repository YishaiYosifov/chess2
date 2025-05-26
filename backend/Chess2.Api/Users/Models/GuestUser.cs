namespace Chess2.Api.Users.Models;

public class GuestUser : IUser
{
    public required string Id { get; set; }
}
