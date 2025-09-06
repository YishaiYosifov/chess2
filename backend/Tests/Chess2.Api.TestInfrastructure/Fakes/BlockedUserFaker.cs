using Bogus;
using Chess2.Api.Profile.Entities;
using Chess2.Api.Profile.Models;
using Chess2.Api.Social.Entities;

namespace Chess2.Api.TestInfrastructure.Fakes;

public class BlockedUserFaker : Faker<BlockedUser>
{
    public BlockedUserFaker(UserId? forUser = null, AuthedUser? blockedUser = null)
    {
        StrictMode(true);
        RuleFor(x => x.Id, 0);

        RuleFor(x => x.UserId, f => (string)(forUser ?? f.Random.Guid().ToString()));

        RuleFor(x => x.Blocked, f => blockedUser ?? new AuthedUserFaker().Generate());
        RuleFor(x => x.BlockedUserId, (f, x) => x.Blocked.Id);
    }
}
