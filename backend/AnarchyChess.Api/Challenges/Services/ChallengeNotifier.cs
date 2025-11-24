using AnarchyChess.Api.Challenges.Models;
using AnarchyChess.Api.Challenges.SignalR;
using AnarchyChess.Api.Game.Models;
using AnarchyChess.Api.Profile.Models;
using AnarchyChess.Api.Shared.Models;
using Microsoft.AspNetCore.SignalR;

namespace AnarchyChess.Api.Challenges.Services;

public interface IChallengeNotifier
{
    Task NotifyChallengeAccepted(GameToken gameToken, ChallengeToken challengeToken);
    Task NotifyChallengeCancelled(
        UserId? cancelledBy,
        UserId? recipientId,
        ChallengeToken challengeToken
    );
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

    public async Task NotifyChallengeCancelled(
        UserId? cancelledBy,
        UserId? recipientId,
        ChallengeToken challengeToken
    )
    {
        await _hub
            .Clients.Group(challengeToken)
            .ChallengeCancelledAsync(cancelledBy, challengeToken);

        if (recipientId is not null)
        {
            await _hub
                .Clients.User(recipientId)
                .ChallengeCancelledAsync(cancelledBy, challengeToken);
        }
    }

    public Task NotifyChallengeAccepted(GameToken gameToken, ChallengeToken challengeToken) =>
        _hub.Clients.Group(challengeToken).ChallengeAcceptedAsync(gameToken, challengeToken);

    public Task SubscribeToChallengeAsync(
        ConnectionId connectionId,
        ChallengeToken challengeToken
    ) => _hub.Groups.AddToGroupAsync(connectionId, challengeToken);
}
