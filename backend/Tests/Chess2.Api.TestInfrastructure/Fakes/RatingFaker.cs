using Bogus;
using Chess2.Api.Game.Models;
using Chess2.Api.UserRating.Entities;
using Chess2.Api.Users.Entities;

namespace Chess2.Api.TestInfrastructure.Fakes;

public class RatingFaker : Faker<Rating>
{
    public RatingFaker(AuthedUser user)
    {
        StrictMode(true)
            .RuleFor(x => x.Id, 0)
            .RuleFor(x => x.UserId, user.Id)
            .RuleFor(x => x.User, user)
            .RuleFor(x => x.TimeControl, f => f.PickRandom<TimeControl>())
            .RuleFor(x => x.Value, f => f.Random.Number(100, 3000));
    }
}
