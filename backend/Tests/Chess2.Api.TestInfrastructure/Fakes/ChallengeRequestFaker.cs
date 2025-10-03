using Chess2.Api.Challenges.Models;
using Chess2.Api.GameSnapshot.Models;

namespace Chess2.Api.TestInfrastructure.Fakes;

public class ChallengeRequestFaker : RecordFaker<ChallengeRequest>
{
    public ChallengeRequestFaker()
    {
        StrictMode(true);
        RuleFor(x => x.ChallengeId, f => (ChallengeId)f.Random.Guid().ToString()[..16]);
        RuleFor(x => x.Requester, f => new MinimalProfileFaker().Generate());
        RuleFor(x => x.Recipient, f => new MinimalProfileFaker().Generate());
        RuleFor(x => x.TimeControl, f => f.PickRandom<TimeControl>());
        RuleFor(x => x.Pool, f => new PoolKeyFaker().Generate());
        RuleFor(x => x.ExpiresAt, f => f.Date.Future());
    }
}
