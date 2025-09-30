using Chess2.Api.Challenges.Models;
using Chess2.Api.GameSnapshot.Models;

namespace Chess2.Api.TestInfrastructure.Fakes;

public class IncomingChallengeFaker : RecordFaker<IncomingChallenge>
{
    public IncomingChallengeFaker()
    {
        StrictMode(true);
        RuleFor(x => x.ChallengeId, f => (ChallengeId)f.Random.Guid().ToString()[..16]);
        RuleFor(x => x.Requester, f => new MinimalProfileFaker().Generate());
        RuleFor(
            x => x.TimeControl,
            f => new TimeControlSettings(
                BaseSeconds: f.Random.Number(100, 1000),
                IncrementSeconds: f.Random.Number(10, 100)
            )
        );
        RuleFor(x => x.ExpiresAt, f => f.Date.Future());
    }
}
