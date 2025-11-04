using Chess2.Api.Challenges.Models;
using Chess2.Api.Challenges.SignalR;
using Chess2.Api.Game.Models;
using Chess2.Api.Profile.Models;
using Chess2.Api.Shared.Models;
using Microsoft.AspNetCore.SignalR;

namespace Chess2.Api.Challenges.Services;

public interface IChallengeNotifier
{
    Task NotifyChallengeAccepted(GameToken gameToken, ChallengeToken challengeToken);
    Task NotifyChallengeCancelled(UserId? cancelledBy, ChallengeToken challengeToken);
    Task NotifyChallengeReceived(ConnectionId recipientConnectionId, ChallengeRequest challenge);
    Task NotifyChallengeReceived(UserId recipientId, ChallengeRequest challenge);
    Task SubscribeToChallengeAsync(ConnectionId connectionId, ChallengeToken challengeToken);
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

    public Task NotifyChallengeCancelled(UserId? cancelledBy, ChallengeToken challengeToken) =>
        _hub.Clients.Group(challengeToken).ChallengeCancelledAsync(cancelledBy, challengeToken);

    public Task NotifyChallengeAccepted(GameToken gameToken, ChallengeToken challengeToken) =>
        _hub.Clients.Group(challengeToken).ChallengeAcceptedAsync(gameToken, challengeToken);

    public Task SubscribeToChallengeAsync(
        ConnectionId connectionId,
        ChallengeToken challengeToken
    ) => _hub.Groups.AddToGroupAsync(connectionId, challengeToken);
}
