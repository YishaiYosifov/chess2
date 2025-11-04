using System.Threading.Channels;
using Chess2.Api.Challenges.Models;
using Chess2.Api.Game.Models;
using Chess2.Api.Profile.Models;
using Chess2.Api.TestInfrastructure.Utils;
using Microsoft.AspNetCore.SignalR.Client;

namespace Chess2.Api.TestInfrastructure.SignalRClients;

public class ChallengeHubClient : BaseHubClient
{
    public const string Path = $"/api/hub/challenge";

    private readonly Channel<ChallengeRequest> _incomingChallengesChannel =
        Channel.CreateUnbounded<ChallengeRequest>();

    private readonly Channel<(
        UserId CancelledBy,
        ChallengeToken ChallengeToken
    )> _cancelledChallengesChannel = Channel.CreateUnbounded<(
        UserId CancelledBy,
        ChallengeToken ChallengeToken
    )>();

    private readonly Channel<(
        GameToken GameToken,
        ChallengeToken ChallengeToken
    )> _acceptedChallengesChannel = Channel.CreateUnbounded<(
        GameToken GameToken,
        ChallengeToken ChallengeToken
    )>();

    public ChallengeHubClient(HubConnection connection)
        : base(connection)
    {
        connection.On<ChallengeRequest>(
            "ChallengeReceivedAsync",
            challenge => _incomingChallengesChannel.Writer.TryWrite(challenge)
        );

        connection.On<UserId, ChallengeToken>(
            "ChallengeCancelledAsync",
            (cancelledBy, challengeToken) =>
                _cancelledChallengesChannel.Writer.TryWrite((cancelledBy, challengeToken))
        );

        connection.On<string, ChallengeToken>(
            "ChallengeAcceptedAsync",
            (gameToken, challengeToken) =>
                _acceptedChallengesChannel.Writer.TryWrite((gameToken, challengeToken))
        );
    }

    public async Task<ChallengeRequest> GetNextIncomingRequestAsync(
        CancellationToken token = default
    )
    {
        using var cts = token.WithTimeout(TimeSpan.FromSeconds(10));
        var challenge = await _incomingChallengesChannel.Reader.ReadAsync(cts.Token);
        return challenge;
    }
}
