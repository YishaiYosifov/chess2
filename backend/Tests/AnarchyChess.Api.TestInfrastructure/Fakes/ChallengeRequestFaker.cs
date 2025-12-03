using AnarchyChess.Api.Challenges.Models;

namespace AnarchyChess.Api.TestInfrastructure.Fakes;

public class ChallengeRequestFaker : RecordFaker<ChallengeRequest>
{
    public ChallengeRequestFaker()
    {
        StrictMode(true);
        RuleFor(x => x.ChallengeToken, f => (ChallengeToken)f.Random.AlphaNumeric(16));
        RuleFor(x => x.Requester, f => new MinimalProfileFaker().Generate());
        RuleFor(x => x.Recipient, f => new MinimalProfileFaker().Generate());
        RuleFor(x => x.Pool, f => new PoolKeyFaker().Generate());
        RuleFor(x => x.ExpiresAt, f => f.Date.Future());
    }
}
