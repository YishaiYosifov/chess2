using AnarchyChess.Api.GameSnapshot.Models;
using AnarchyChess.Api.Matchmaking.Models;
using AnarchyChess.Api.Profile.Models;

namespace AnarchyChess.Api.TestInfrastructure.Fakes;

public class OpenRatedSeekerFaker : RecordFaker<OpenRatedSeeker>
{
    public OpenRatedSeekerFaker(UserId? userId = null)
    {
        StrictMode(true);
        RuleFor(x => x.UserId, f => userId ?? new UserId(f.Random.Guid().ToString()));
        RuleFor(x => x.UserName, f => f.Internet.UserName());
        RuleFor(x => x.ExcludeUserIds, []);
        RuleFor(x => x.CreatedAt, f => DateTime.UtcNow);
        RuleFor(
            x => x.Ratings,
            f =>
                Enum.GetValues<TimeControl>()
                    .ToDictionary(x => x, x => f.Random.Number(min: 100, max: 3000))
        );
    }
}
