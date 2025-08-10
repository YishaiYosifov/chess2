using Chess2.Api.Matchmaking.Models;
using Chess2.Api.Users.Models;

namespace Chess2.Api.TestInfrastructure.Fakes;

public class RatedSeekerFaker : RecordFaker<RatedSeeker>
{
    public RatedSeekerFaker()
    {
        StrictMode(true);
        RuleFor(x => x.UserId, f => new UserId(f.Random.Guid().ToString()));
        RuleFor(x => x.UserName, f => f.Internet.UserName());
        RuleFor(x => x.BlockedUserIds, []);
        RuleFor(x => x.Rating, f => new SeekerRatingFaker().Generate());
        RuleFor(x => x.CreatedAt, f => DateTime.UtcNow);
    }
}
