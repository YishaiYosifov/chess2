using Chess2.Api.Challenges.Models;
using Orleans.Concurrency;

namespace Chess2.Api.Challenges.Grains;

[Alias("Chess2.Api.Challenges.Grains.IChallengeInboxGrain")]
public interface IChallengeInboxGrain : IGrainWithStringKey
{
    [Alias("GetIncomingChallengesAsync")]
    Task<List<IncomingChallenge>> GetIncomingChallengesAsync();

    [Alias("ChallengeCreatedAsync")]
    [OneWay]
    Task ChallengeCreatedAsync(IncomingChallenge challenge);

    [Alias("ChallengeCanceledAsync")]
    [OneWay]
    Task ChallengeCanceledAsync(ChallengeId challengeId);
}

public class ChallengeInboxGrain : Grain, IChallengeInboxGrain
{
    private readonly Dictionary<ChallengeId, IncomingChallenge> _incomingChallenges = [];

    public Task<List<IncomingChallenge>> GetIncomingChallengesAsync() =>
        Task.FromResult(_incomingChallenges.Values.ToList());

    public Task ChallengeCreatedAsync(IncomingChallenge challenge)
    {
        _incomingChallenges[challenge.ChallengeId] = challenge;
        return Task.CompletedTask;
    }

    public Task ChallengeCanceledAsync(ChallengeId challengeId)
    {
        _incomingChallenges.Remove(challengeId);
        return Task.CompletedTask;
    }
}
