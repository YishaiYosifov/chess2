using Chess2.Api.Challenges.Grains;
using Chess2.Api.Challenges.Models;
using Chess2.Api.Infrastructure;
using Chess2.Api.Infrastructure.SignalR;
using Chess2.Api.Matchmaking.Models;
using Chess2.Api.Profile.Models;
using ErrorOr;
using Microsoft.AspNetCore.Authorization;

namespace Chess2.Api.Challenges.SignalR;

public interface IChallengeHubClient : IChess2HubClient
{
    public Task ChallengeReceivedAsync(ChallengeRequest challenge);
    public Task ChallengeCancelledAsync(ChallengeId challengeId);
    public Task ChallengeAcceptedAsync(string gameToken, ChallengeId challengeId);
}

[Authorize(AuthPolicies.ActiveSession)]
public class ChallengeHub(IGrainFactory grains) : Chess2Hub<IChallengeHubClient>
{
    private readonly IGrainFactory _grains = grains;

    public async Task CreateChallengeAsync(UserId recipientId, PoolKey pool)
    {
        if (!TryGetUserId(out var requesterId))
        {
            await HandleErrors(Error.Unauthorized());
            return;
        }

        var id = Guid.NewGuid().ToString()[..16];
        var challengeGrain = _grains.GetGrain<ChallengeGrain>(id);
        var result = await challengeGrain.CreateAsync(requesterId, recipientId, pool);
        if (result.IsError)
            await HandleErrors(result.Errors);
    }
}
