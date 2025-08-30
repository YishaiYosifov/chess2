using Chess2.Api.GameSnapshot.Models;
using Chess2.Api.Matchmaking.Models;
using Chess2.Api.Profile.Models;

namespace Chess2.Api.TestInfrastructure.Fakes;

public class OpenRatedSeekerFaker : RecordFaker<OpenRatedSeeker>
{
    public OpenRatedSeekerFaker(UserId? userId = null)
    {
        StrictMode(true);
        RuleFor(x => x.UserId, f => userId ?? new UserId(f.Random.Guid().ToString()));
        RuleFor(x => x.UserName, f => f.Internet.UserName());
        RuleFor(x => x.BlockedUserIds, []);
        RuleFor(x => x.CreatedAt, f => DateTime.UtcNow);
        RuleFor(
            x => x.Ratings,
            f =>
                Enum.GetValues<TimeControl>()
                    .ToDictionary(x => x, x => f.Random.Number(min: 100, max: 3000))
        );
    }
}
