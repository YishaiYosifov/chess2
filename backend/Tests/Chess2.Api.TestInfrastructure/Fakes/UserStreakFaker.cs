using Bogus;
using Chess2.Api.Streaks.Entities;

namespace Chess2.Api.TestInfrastructure.Fakes;

public class UserStreakFaker : Faker<UserStreak>
{
    public UserStreakFaker()
    {
        StrictMode(true);
        RuleFor(x => x.User, f => new AuthedUserFaker().Generate());
        RuleFor(x => x.UserId, (f, x) => x.User.Id);
        RuleFor(x => x.CurrentStreak, f => f.Random.Number(0, 100));
        RuleFor(x => x.HighestStreak, f => f.Random.Number(0, 100));
    }
}
