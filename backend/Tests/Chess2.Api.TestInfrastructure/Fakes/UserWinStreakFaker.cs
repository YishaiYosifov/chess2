using Bogus;
using Chess2.Api.Streaks.Entities;

namespace Chess2.Api.TestInfrastructure.Fakes;

public class UserWinStreakFaker : Faker<UserWinStreak>
{
    public UserWinStreakFaker(int? currentStreak = null, int? highestStreak = null)
    {
        StrictMode(true);
        RuleFor(x => x.User, f => new AuthedUserFaker().Generate());
        RuleFor(x => x.UserId, (f, x) => x.User.Id);

        RuleFor(
            x => x.CurrentStreakGames,
            f =>
                [
                    .. Enumerable
                        .Range(0, currentStreak ?? f.Random.Number(0, 20))
                        .Select(_ => f.Random.AlphaNumeric(16)),
                ]
        );
        RuleFor(
            x => x.HighestStreakGames,
            f =>
                [
                    .. Enumerable
                        .Range(0, highestStreak ?? f.Random.Number(0, 20))
                        .Select(_ => f.Random.AlphaNumeric(16)),
                ]
        );
    }
}
