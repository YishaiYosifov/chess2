using Chess2.Api.Challenges.Models;
using Chess2.Api.Infrastructure;
using Chess2.Api.Infrastructure.SignalR;
using Microsoft.AspNetCore.Authorization;

namespace Chess2.Api.Challenges.SignalR;

public interface IChallengeHubClient : IChess2HubClient
{
    public Task ChallengeReceivedAsync(ChallengeId challengeId);
    public Task ChallengeCanceledAsync(ChallengeId challengeId);
}

[Authorize(AuthPolicies.ActiveSession)]
public class ChallengeHub : Chess2Hub<IChallengeHubClient> { }
