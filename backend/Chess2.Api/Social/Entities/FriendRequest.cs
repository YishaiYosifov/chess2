using System.ComponentModel.DataAnnotations.Schema;
using Chess2.Api.Profile.Entities;

namespace Chess2.Api.Social.Entities;

public class FriendRequest
{
    public int Id { get; set; }

    public required string RequesterUserId { get; set; }

    [ForeignKey(nameof(RequesterUserId))]
    public required AuthedUser Requester { get; set; }

    public required string RecipientUserId { get; set; }

    [ForeignKey(nameof(RecipientUserId))]
    public required AuthedUser Recipient { get; set; }
}
