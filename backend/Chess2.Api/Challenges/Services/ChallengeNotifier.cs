using Chess2.Api.Challenges.Models;
using Chess2.Api.Challenges.SignalR;
using Chess2.Api.LiveGame.Models;
using Chess2.Api.Profile.Models;
using Chess2.Api.Shared.Models;
using Microsoft.AspNetCore.SignalR;

namespace Chess2.Api.Challenges.Services;

public interface IChallengeNotifier
{
    Task NotifyChallengeAccepted(GameToken gameToken, ChallengeId challengeId);
    Task NotifyChallengeCancelled(UserId? cancelledBy, ChallengeId challengeId);
    Task NotifyChallengeReceived(ConnectionId recipientConnectionId, ChallengeRequest challenge);
    Task NotifyChallengeReceived(UserId recipientId, ChallengeRequest challenge);
    Task SubscribeToChallengeAsync(ConnectionId connectionId, ChallengeId challengeId);
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

    public Task NotifyChallengeCancelled(UserId? cancelledBy, ChallengeId challengeId) =>
        _hub.Clients.Group(challengeId).ChallengeCancelledAsync(cancelledBy, challengeId);

    public Task NotifyChallengeAccepted(GameToken gameToken, ChallengeId challengeId) =>
        _hub.Clients.Group(challengeId).ChallengeAcceptedAsync(gameToken, challengeId);

    public Task SubscribeToChallengeAsync(ConnectionId connectionId, ChallengeId challengeId) =>
        _hub.Groups.AddToGroupAsync(connectionId, challengeId);
}
