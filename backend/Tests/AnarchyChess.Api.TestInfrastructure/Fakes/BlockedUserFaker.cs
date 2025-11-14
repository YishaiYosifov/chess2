using Bogus;
using AnarchyChess.Api.Profile.Entities;
using AnarchyChess.Api.Profile.Models;
using AnarchyChess.Api.Social.Entities;

namespace AnarchyChess.Api.TestInfrastructure.Fakes;

public class BlockedUserFaker : Faker<BlockedUser>
{
    public BlockedUserFaker(UserId? forUser = null, AuthedUser? blockedUser = null)
    {
        StrictMode(true);
        RuleFor(x => x.Id, 0);

        RuleFor(x => x.UserId, f => forUser ?? f.Random.Guid().ToString());

        RuleFor(x => x.Blocked, f => blockedUser ?? new AuthedUserFaker().Generate());
        RuleFor(x => x.BlockedUserId, (f, x) => x.Blocked.Id);
    }
}
