using Bogus;
using Chess2.Api.Profile.Entities;
using Chess2.Api.Social.Entities;

namespace Chess2.Api.TestInfrastructure.Fakes;

public class FriendFaker : Faker<Friend>
{
    public FriendFaker(AuthedUser? user1 = null, AuthedUser? user2 = null)
    {
        StrictMode(true);
        RuleFor(x => x.Id, 0);

        RuleFor(x => x.User1, f => user1 ?? new AuthedUserFaker().Generate());
        RuleFor(x => x.UserId1, (f, x) => x.User1.Id);

        RuleFor(x => x.User2, f => user2 ?? new AuthedUserFaker().Generate());
        RuleFor(x => x.UserId2, (f, x) => x.User2.Id);
    }
}
