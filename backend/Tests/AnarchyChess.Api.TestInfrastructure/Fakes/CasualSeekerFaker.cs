using AnarchyChess.Api.Matchmaking.Models;
using AnarchyChess.Api.Profile.Models;

namespace AnarchyChess.Api.TestInfrastructure.Fakes;

public class CasualSeekerFaker : RecordFaker<CasualSeeker>
{
    public CasualSeekerFaker(UserId? userId = null)
    {
        StrictMode(true);
        RuleFor(x => x.UserId, f => userId ?? new UserId(f.Random.Guid().ToString()));
        RuleFor(x => x.UserName, f => f.Internet.UserName());
        RuleFor(x => x.ExcludeUserIds, []);
        RuleFor(x => x.CreatedAt, f => DateTime.UtcNow);
    }
}
