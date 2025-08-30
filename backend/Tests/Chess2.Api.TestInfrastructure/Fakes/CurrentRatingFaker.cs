using Bogus;
using Chess2.Api.GameSnapshot.Models;
using Chess2.Api.UserRating.Entities;
using Chess2.Api.Profile.Entities;

namespace Chess2.Api.TestInfrastructure.Fakes;

public class CurrentRatingFaker : Faker<CurrentRating>
{
    public CurrentRatingFaker(AuthedUser user, int? rating = null, TimeControl? timeControl = null)
    {
        StrictMode(true);
        RuleFor(x => x.Id, 0);
        RuleFor(x => x.UserId, user.Id);
        RuleFor(x => x.TimeControl, f => timeControl ?? f.PickRandom<TimeControl>());
        RuleFor(x => x.Value, f => rating ?? f.Random.Number(100, 3000));
        RuleFor(x => x.LastUpdated, f => DateTime.UtcNow.AddDays(f.IndexFaker));
    }
}
