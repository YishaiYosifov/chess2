using AnarchyChess.Api.Challenges.Grains;
using AnarchyChess.Api.Challenges.Models;
using AnarchyChess.Api.Challenges.Services;
using AnarchyChess.Api.Game.Models;
using AnarchyChess.Api.Infrastructure;
using AnarchyChess.Api.Infrastructure.SignalR;
using AnarchyChess.Api.Profile.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace AnarchyChess.Api.Challenges.SignalR;

public interface IChallengeHubClient : IAnarchyChessHubClient
{
    public Task ChallengeReceivedAsync(ChallengeRequest challenge);
    public Task ChallengeCancelledAsync(UserId? cancelledBy, ChallengeToken challengeToken);
    public Task ChallengeAcceptedAsync(GameToken gameToken, ChallengeToken challengeToken);
}

[Authorize(AuthPolicies.ActiveSession)]
public class ChallengeHub(IGrainFactory grains, IChallengeNotifier challengeNotifier)
    : AnarchyChessHub<IChallengeHubClient>
{
    private const string ChallengeTokenQueryParam = "challengeToken";

    private readonly IChallengeNotifier _challengeNotifier = challengeNotifier;
    private readonly IGrainFactory _grains = grains;

    public override async Task OnConnectedAsync()
    {
        try
        {
            if (!TryGetUserId(out var userId))
                return;

            ChallengeToken? challengeToken = Context
                .GetHttpContext()
                ?.Request.Query[ChallengeTokenQueryParam]
                .ToString();
            if (!string.IsNullOrWhiteSpace(challengeToken))
                await SubscribeToChallenge(userId, challengeToken.Value);
            else
                await NotifyOfIncoming(userId);
        }
        finally
        {
            await base.OnConnectedAsync();
        }
    }

    private async Task SubscribeToChallenge(UserId userId, ChallengeToken challengeToken)
    {
        var challengeGrain = _grains.GetGrain<IChallengeGrain>(challengeToken);
        var result = await challengeGrain.SubscribeAsync(userId, Context.ConnectionId);
        if (result.IsError)
            await HandleErrors(result.Errors);
    }

    private async Task NotifyOfIncoming(UserId userId)
    {
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
}
