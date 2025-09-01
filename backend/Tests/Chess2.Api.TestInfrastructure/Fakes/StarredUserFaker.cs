using Bogus;
using Chess2.Api.Profile.Entities;
using Chess2.Api.Profile.Models;
using Chess2.Api.Social.Entities;

namespace Chess2.Api.TestInfrastructure.Fakes;

public class StarredUserFaker : Faker<StarredUser>
{
    public StarredUserFaker(UserId? forUser = null, AuthedUser? starredUser = null)
    {
        StrictMode(true);
        RuleFor(x => x.Id, 0);

        RuleFor(x => x.UserId, f => (string)(forUser ?? f.Random.Guid().ToString()));

        RuleFor(x => x.Starred, f => starredUser ?? new AuthedUserFaker().Generate());
        RuleFor(x => x.StarredUserId, (f, x) => x.Starred.Id);
    }
}
