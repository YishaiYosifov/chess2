namespace Chess2.Api.Users.Entities;

public class Friend
{
    public int Id { get; set; }
    public required string UserId { get; set; }
    public required string FriendUserId { get; set; }
}
