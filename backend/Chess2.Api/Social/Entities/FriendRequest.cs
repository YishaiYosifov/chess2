namespace Chess2.Api.Social.Entities;

public class FriendRequest
{
    public int Id { get; set; }

    public required string RequesterUserId { get; set; }
    public required string RecipientUserId { get; set; }
}
