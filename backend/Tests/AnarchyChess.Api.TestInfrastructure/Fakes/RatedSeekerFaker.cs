using AnarchyChess.Api.Matchmaking.Models;
using AnarchyChess.Api.Profile.Models;

namespace AnarchyChess.Api.TestInfrastructure.Fakes;

public class RatedSeekerFaker : RecordFaker<RatedSeeker>
{
    public RatedSeekerFaker(UserId? userId = null, int? rating = null)
    {
        StrictMode(true);
        RuleFor(x => x.UserId, f => userId ?? new UserId(f.Random.Guid().ToString()));
        RuleFor(x => x.UserName, f => f.Internet.UserName());
        RuleFor(x => x.ExcludeUserIds, []);
        RuleFor(x => x.Rating, f => new SeekerRatingFaker(rating).Generate());
        RuleFor(x => x.CreatedAt, f => DateTime.UtcNow);
    }
}
