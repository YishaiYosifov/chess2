using AnarchyChess.Api.GameSnapshot.Models;
using AnarchyChess.Api.Matchmaking.Models;

namespace AnarchyChess.Api.TestInfrastructure.Fakes;

public class SeekerRatingFaker : RecordFaker<SeekerRating>
{
    public SeekerRatingFaker(int? rating = null, TimeControl? timeControl = null)
    {
        StrictMode(true);
        RuleFor(x => x.Value, f => rating ?? f.Random.Number(100, 3000));
        RuleFor(x => x.AllowedRatingRange, f => f.Random.Number(100, 400));
        RuleFor(x => x.TimeControl, f => timeControl ?? f.PickRandom<TimeControl>());
    }
}
