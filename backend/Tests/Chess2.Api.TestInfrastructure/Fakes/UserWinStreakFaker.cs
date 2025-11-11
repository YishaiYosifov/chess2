using Bogus;
using Chess2.Api.Streaks.Entities;

namespace Chess2.Api.TestInfrastructure.Fakes;

public class UserWinStreakFaker : Faker<UserWinStreak>
{
    public UserWinStreakFaker()
    {
        StrictMode(true);
        RuleFor(x => x.User, f => new AuthedUserFaker().Generate());
        RuleFor(x => x.UserId, (f, x) => x.User.Id);

        RuleFor(x => x.CurrentStreak, f => f.Random.Number(0, 100));
        RuleFor(
            x => x.CurrentStreakGameTokens,
            (f, x) =>
                [.. Enumerable.Range(0, x.CurrentStreak).Select(_ => f.Random.AlphaNumeric(16))]
        );

        RuleFor(x => x.HighestStreak, f => f.Random.Number(0, 100));
        RuleFor(
            x => x.HighestStreakGameTokens,
            (f, x) =>
                [.. Enumerable.Range(0, x.HighestStreak).Select(_ => f.Random.AlphaNumeric(16))]
        );
    }
}
