namespace Chess2.Api.Social.Entities;

public class BlockedUser
{
    public int Id { get; set; }
    public required string UserId { get; set; }
    public required string BlockedUserId { get; set; }
}
