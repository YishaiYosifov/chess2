using Bogus;
using Chess2.Api.Models.Entities;

namespace Chess2.Api.TestInfrastructure.Fakes;

public class RatingFaker : Faker<Rating>
{
    public RatingFaker(AuthedUser user)
    {
        StrictMode(true)
            .RuleFor(x => x.RatingId, 0)
            .RuleFor(x => x.UserId, user.AuthedUserId)
            .RuleFor(x => x.User, user)
            .RuleFor(x => x.Value, f => f.Random.Number(100, 3000));
    }
}
