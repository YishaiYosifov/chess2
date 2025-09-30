using Chess2.Api.Challenges.Models;
using Orleans.Concurrency;

namespace Chess2.Api.Challenges.Grains;

[Alias("Chess2.Api.Challenges.Grains.IChallengeInboxGrain")]
public interface IChallengeInboxGrain : IGrainWithStringKey
{
    [Alias("GetIncomingChallengesAsync")]
    [OneWay]
    Task<List<ChallengeRequest>> GetIncomingChallengesAsync();

    [Alias("RecordChallengeCreatedAsync")]
    [OneWay]
    Task RecordChallengeCreatedAsync(ChallengeRequest challenge);

    [Alias("RecordChallengeRemovedAsync")]
    [OneWay]
    Task RecordChallengeRemovedAsync(ChallengeId challengeId);
}

public class ChallengeInboxGrain : Grain, IChallengeInboxGrain
{
    private readonly Dictionary<ChallengeId, ChallengeRequest> _incomingChallenges = [];

    public Task<List<ChallengeRequest>> GetIncomingChallengesAsync() =>
        Task.FromResult(_incomingChallenges.Values.ToList());

    public Task RecordChallengeCreatedAsync(ChallengeRequest challenge) =>
        Task.FromResult(_incomingChallenges[challenge.ChallengeId] = challenge);

    public Task RecordChallengeRemovedAsync(ChallengeId challengeId) =>
        Task.FromResult(_incomingChallenges.Remove(challengeId));
}
