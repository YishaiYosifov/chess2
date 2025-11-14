using Bogus;
using AnarchyChess.Api.Profile.Entities;
using AnarchyChess.Api.Profile.Models;
using AnarchyChess.Api.Social.Entities;

namespace AnarchyChess.Api.TestInfrastructure.Fakes;

public class StarredUserFaker : Faker<StarredUser>
{
    public StarredUserFaker(UserId? forUser = null, AuthedUser? starredUser = null)
    {
        StrictMode(true);
        RuleFor(x => x.Id, 0);

        RuleFor(x => x.UserId, f => forUser ?? f.Random.Guid().ToString());

        RuleFor(x => x.Starred, f => starredUser ?? new AuthedUserFaker().Generate());
        RuleFor(x => x.StarredUserId, (f, x) => x.Starred.Id);
    }
}
