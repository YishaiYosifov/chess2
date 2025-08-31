using Bogus;
using Chess2.Api.Profile.Entities;
using Chess2.Api.Social.Entities;

namespace Chess2.Api.TestInfrastructure.Fakes;

public class FriendRequestFaker : Faker<FriendRequest>
{
    public FriendRequestFaker(AuthedUser? requester = null, AuthedUser? recipient = null)
    {
        StrictMode(true);
        RuleFor(x => x.Id, 0);

        RuleFor(x => x.Requester, f => requester ?? new AuthedUserFaker().Generate());
        RuleFor(x => x.RequesterUserId, (f, x) => x.Requester.Id);

        RuleFor(x => x.Recipient, f => recipient ?? new AuthedUserFaker().Generate());
        RuleFor(x => x.RecipientUserId, (f, x) => x.Recipient.Id);
    }
}
