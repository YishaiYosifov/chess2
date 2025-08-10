using Chess2.Api.Matchmaking.Models;

namespace Chess2.Api.TestInfrastructure.Fakes;

public class SeekerRatingFaker : RecordFaker<SeekerRating>
{
    public SeekerRatingFaker(int? rating = null)
    {
        StrictMode(true);
        RuleFor(x => x.Value, f => rating ?? f.Random.Number(100, 3000));
        RuleFor(x => x.AllowedRatingRange, f => f.Random.Number(100, 400));
    }
}
