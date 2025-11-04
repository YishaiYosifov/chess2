using Chess2.Api.Challenges.Models;
using Orleans.Concurrency;

namespace Chess2.Api.Challenges.Grains;

[Alias("Chess2.Api.Challenges.Grains.IChallengeInboxGrain")]
public interface IChallengeInboxGrain : IGrainWithStringKey
{
    [Alias("GetIncomingChallengesAsync")]
    Task<List<ChallengeRequest>> GetIncomingChallengesAsync();

    [Alias("RecordChallengeCreatedAsync")]
    Task RecordChallengeCreatedAsync(ChallengeRequest challenge);

    [Alias("RecordChallengeRemovedAsync")]
    [OneWay]
    Task RecordChallengeRemovedAsync(ChallengeToken challengeToken);
}

public class ChallengeInboxGrain : Grain, IChallengeInboxGrain
{
    private readonly Dictionary<ChallengeToken, ChallengeRequest> _incomingChallenges = [];

    public Task<List<ChallengeRequest>> GetIncomingChallengesAsync() =>
        Task.FromResult(_incomingChallenges.Values.ToList());

    public Task RecordChallengeCreatedAsync(ChallengeRequest challenge) =>
        Task.FromResult(_incomingChallenges[challenge.ChallengeToken] = challenge);

    public Task RecordChallengeRemovedAsync(ChallengeToken challengeToken) =>
        Task.FromResult(_incomingChallenges.Remove(challengeToken));
}
