using Bogus;
using Chess2.Api.GameSnapshot.Models;
using Chess2.Api.UserRating.Entities;
using Chess2.Api.Users.Entities;

namespace Chess2.Api.TestInfrastructure.Fakes;

public class RatingArchiveFaker : Faker<RatingArchive>
{
    public RatingArchiveFaker(AuthedUser user, int? rating = null, TimeControl? timeControl = null)
    {
        StrictMode(true);
        RuleFor(x => x.Id, 0);
        RuleFor(x => x.UserId, user.Id);
        RuleFor(x => x.TimeControl, f => timeControl ?? f.PickRandom<TimeControl>());
        RuleFor(x => x.Value, f => rating ?? f.Random.Number(100, 3000));
        RuleFor(x => x.AchievedAt, f => DateTime.UtcNow.AddDays(f.IndexFaker));
    }
}
