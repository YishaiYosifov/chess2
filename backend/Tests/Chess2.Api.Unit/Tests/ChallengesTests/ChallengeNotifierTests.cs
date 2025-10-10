using Chess2.Api.Challenges.Models;
using Chess2.Api.Challenges.Services;
using Chess2.Api.Challenges.SignalR;
using Chess2.Api.LiveGame.Models;
using Chess2.Api.Profile.Models;
using Chess2.Api.Shared.Models;
using Chess2.Api.TestInfrastructure.Fakes;
using Microsoft.AspNetCore.SignalR;
using NSubstitute;

namespace Chess2.Api.Unit.Tests.ChallengesTests;

public class ChallengeNotifierTests
{
    private readonly UserId _requesterId = "test requester id";
    private readonly UserId _recipientId = "test recipient id";

    private readonly IHubContext<ChallengeHub, IChallengeHubClient> _hubContextMock =
        Substitute.For<IHubContext<ChallengeHub, IChallengeHubClient>>();
    private readonly IHubClients<IChallengeHubClient> _clientsMock = Substitute.For<
        IHubClients<IChallengeHubClient>
    >();

    private readonly IChallengeHubClient _requesterProxyMock =
        Substitute.For<IChallengeHubClient>();
    private readonly IChallengeHubClient _recipientProxyMock =
        Substitute.For<IChallengeHubClient>();

    private readonly ChallengeNotifier _notifier;

    public ChallengeNotifierTests()
    {
        _clientsMock.User(_requesterId).Returns(_requesterProxyMock);
        _clientsMock.User(_recipientId).Returns(_recipientProxyMock);
        _hubContextMock.Clients.Returns(_clientsMock);

        _notifier = new(_hubContextMock);
    }

    [Fact]
    public async Task NotifyChallengeReceivedAsync_with_a_user_id_notifies_the_correct_client()
    {
        var challenge = new ChallengeRequestFaker().Generate();

        await _notifier.NotifyChallengeReceived(_recipientId, challenge);

        await _recipientProxyMock.Received(1).ChallengeReceivedAsync(challenge);
    }

    [Fact]
    public async Task NotifyChallengeReceivedAsync_with_a_connection_id_notifies_the_correct_client()
    {
        ConnectionId connId = "test-connection";
        var clientConnProxyMock = Substitute.For<IChallengeHubClient>();
        _clientsMock.Client(connId).Returns(clientConnProxyMock);

        var challenge = new ChallengeRequestFaker().Generate();

        await _notifier.NotifyChallengeReceived(connId, challenge);

        await clientConnProxyMock.Received(1).ChallengeReceivedAsync(challenge);
    }

    [Fact]
    public async Task NotifyChallengeCancelledAsync_notifies_both_requester_and_recipient_clients()
    {
        ChallengeId challengeId = "challenge-123";
        UserId cancelledBy = "cancelled by";

        await _notifier.NotifyChallengeCancelled(
            cancelledBy,
            _requesterId,
            _recipientId,
            challengeId
        );

        await _requesterProxyMock.Received(1).ChallengeCancelledAsync(cancelledBy, challengeId);
        await _recipientProxyMock.Received(1).ChallengeCancelledAsync(cancelledBy, challengeId);
    }

    [Fact]
    public async Task NotifyChallengeAcceptedAsync_notifies_the_requester_client()
    {
        ChallengeId challengeId = "challenge-accepted";
        GameToken gameToken = "game-token-xyz";

        await _notifier.NotifyChallengeAccepted(_requesterId, gameToken, challengeId);

        await _requesterProxyMock.Received(1).ChallengeAcceptedAsync(gameToken, challengeId);
    }
}
