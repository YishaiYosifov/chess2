using Chess2.Api.Challenges.Grains;
using Chess2.Api.Challenges.Models;
using Chess2.Api.Challenges.Services;
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
public class ChallengeHub(IGrainFactory grains, IChallengeNotifier challengeNotifier)
    : Chess2Hub<IChallengeHubClient>
{
    private readonly IGrainFactory _grains = grains;
    private readonly IChallengeNotifier _challengeNotifier = challengeNotifier;

    public async Task CreateChallengeAsync(UserId recipientId, PoolKey pool)
    {
        if (!TryGetUserId(out var requesterId))
        {
            await HandleErrors(Error.Unauthorized());
            return;
        }

        var id = Guid.NewGuid().ToString()[..16];
        var challengeGrain = _grains.GetGrain<IChallengeGrain>(id);
        var result = await challengeGrain.CreateAsync(requesterId, recipientId, pool);
        if (result.IsError)
            await HandleErrors(result.Errors);
    }

    public async Task CancelChallengeAsync(ChallengeId challengeId)
    {
        if (!TryGetUserId(out var userId))
        {
            await HandleErrors(Error.Unauthorized());
            return;
        }

        var challengeGrain = _grains.GetGrain<IChallengeGrain>(challengeId);
        var result = await challengeGrain.CancelAsync(cancelledBy: userId);
        if (result.IsError)
            await HandleErrors(result.Errors);
    }

    public override async Task OnConnectedAsync()
    {
        try
        {
            if (!TryGetUserId(out var userId))
                return;

            var challengeInboxGrain = _grains.GetGrain<IChallengeInboxGrain>(userId);
            var incomingChallenges = await challengeInboxGrain.GetIncomingChallengesAsync();
            foreach (var challenge in incomingChallenges)
            {
                await _challengeNotifier.NotifyChallengeReceived(
                    recipientConnectionId: Context.ConnectionId,
                    challenge
                );
            }
        }
        finally
        {
            await base.OnConnectedAsync();
        }
    }
}
