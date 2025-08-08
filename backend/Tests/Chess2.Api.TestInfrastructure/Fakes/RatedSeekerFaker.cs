using Chess2.Api.Matchmaking.Models;

namespace Chess2.Api.TestInfrastructure.Fakes;

public class RatedSeekerFaker : RecordFaker<RatedSeeker>
{
    public RatedSeekerFaker()
    {
        StrictMode(true);
        RuleFor(x => x.UserId, f => f.Random.Guid().ToString());
        RuleFor(x => x.UserName, f => f.Internet.UserName());
        RuleFor(x => x.BlockedUserIds, []);
        RuleFor(x => x.Rating, f => new SeekerRatingFaker().Generate());
    }
}
