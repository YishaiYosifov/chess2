using Chess2.Api.Challenges.Models;
using Chess2.Api.Challenges.SignalR;
using Chess2.Api.Profile.Models;
using Chess2.Api.Shared.Models;
using Microsoft.AspNetCore.SignalR;

namespace Chess2.Api.Challenges.Services;

public interface IChallengeNotifier
{
    Task NotifyChallengeReceived(ConnectionId recipientConnectionId, IncomingChallenge challenge);
    Task NotifyChallengeReceived(UserId recipientId, IncomingChallenge challenge);
}

public class ChallengeNotifier(IHubContext<ChallengeHub, IChallengeHubClient> hub)
    : IChallengeNotifier
{
    private readonly IHubContext<ChallengeHub, IChallengeHubClient> _hub = hub;

    public Task NotifyChallengeReceived(UserId recipientId, IncomingChallenge challenge) =>
        _hub.Clients.User(recipientId).ChallengeReceivedAsync(challenge);

    public Task NotifyChallengeReceived(
        ConnectionId recipientConnectionId,
        IncomingChallenge challenge
    ) => _hub.Clients.Client(recipientConnectionId).ChallengeReceivedAsync(challenge);
}
