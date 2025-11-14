using Bogus;
using AnarchyChess.Api.GameSnapshot.Models;
using AnarchyChess.Api.UserRating.Entities;
using AnarchyChess.Api.Profile.Entities;

namespace AnarchyChess.Api.TestInfrastructure.Fakes;

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
