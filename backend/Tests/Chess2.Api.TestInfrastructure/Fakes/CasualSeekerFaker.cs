using Chess2.Api.Matchmaking.Models;
using Chess2.Api.Profile.Models;

namespace Chess2.Api.TestInfrastructure.Fakes;

public class CasualSeekerFaker : RecordFaker<CasualSeeker>
{
    public CasualSeekerFaker(UserId? userId = null)
    {
        StrictMode(true);
        RuleFor(x => x.UserId, f => userId ?? new UserId(f.Random.Guid().ToString()));
        RuleFor(x => x.UserName, f => f.Internet.UserName());
        RuleFor(x => x.BlockedUserIds, []);
        RuleFor(x => x.CreatedAt, f => DateTime.UtcNow);
    }
}
