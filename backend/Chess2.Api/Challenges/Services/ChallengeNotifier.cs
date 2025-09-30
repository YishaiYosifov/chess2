using Chess2.Api.Challenges.Models;
using Chess2.Api.Challenges.SignalR;
using Chess2.Api.Profile.Models;
using Chess2.Api.Shared.Models;
using Microsoft.AspNetCore.SignalR;

namespace Chess2.Api.Challenges.Services;

public interface IChallengeNotifier
{
    Task NotifyChallengeAccepted(UserId requesterId, string gameToken, ChallengeId challengeId);
    Task NotifyChallengeCancelled(UserId requesterId, UserId recipientId, ChallengeId challengeId);
    Task NotifyChallengeReceived(ConnectionId recipientConnectionId, ChallengeRequest challenge);
    Task NotifyChallengeReceived(UserId recipientId, ChallengeRequest challenge);
}

public class ChallengeNotifier(IHubContext<ChallengeHub, IChallengeHubClient> hub)
    : IChallengeNotifier
{
    private readonly IHubContext<ChallengeHub, IChallengeHubClient> _hub = hub;

    public Task NotifyChallengeReceived(UserId recipientId, ChallengeRequest challenge) =>
        _hub.Clients.User(recipientId).ChallengeReceivedAsync(challenge);

    public Task NotifyChallengeReceived(
        ConnectionId recipientConnectionId,
        ChallengeRequest challenge
    ) => _hub.Clients.Client(recipientConnectionId).ChallengeReceivedAsync(challenge);

    public async Task NotifyChallengeCancelled(
        UserId requesterId,
        UserId recipientId,
        ChallengeId challengeId
    )
    {
        await _hub.Clients.User(requesterId).ChallengeCancelledAsync(challengeId);
        await _hub.Clients.User(recipientId).ChallengeCancelledAsync(challengeId);
    }

    public Task NotifyChallengeAccepted(
        UserId requesterId,
        string gameToken,
        ChallengeId challengeId
    ) => _hub.Clients.User(requesterId).ChallengeAcceptedAsync(gameToken, challengeId);
}
