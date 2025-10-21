using Chess2.Api.Challenges.Models;
using Chess2.Api.Game.Models;
using Chess2.Api.Profile.Models;
using Chess2.Api.TestInfrastructure.Utils;
using Microsoft.AspNetCore.SignalR.Client;
using System.Threading.Channels;

namespace Chess2.Api.TestInfrastructure.SignalRClients;

public class ChallengeHubClient : BaseHubClient
{
    public const string Path = $"/api/hub/challenge";

    private readonly Channel<ChallengeRequest> _incomingChallengesChannel =
        Channel.CreateUnbounded<ChallengeRequest>();

    private readonly Channel<(
        UserId CancelledBy,
        ChallengeId ChallengeId
    )> _cancelledChallengesChannel = Channel.CreateUnbounded<(
        UserId CancelledBy,
        ChallengeId ChallengeId
    )>();

    private readonly Channel<(
        GameToken GameToken,
        ChallengeId ChallengeId
    )> _acceptedChallengesChannel = Channel.CreateUnbounded<(
        GameToken GameToken,
        ChallengeId ChallengeId
    )>();

    public ChallengeHubClient(HubConnection connection)
        : base(connection)
    {
        connection.On<ChallengeRequest>(
            "ChallengeReceivedAsync",
            challenge => _incomingChallengesChannel.Writer.TryWrite(challenge)
        );

        connection.On<UserId, ChallengeId>(
            "ChallengeCancelledAsync",
            (cancelledBy, challengeId) =>
                _cancelledChallengesChannel.Writer.TryWrite((cancelledBy, challengeId))
        );

        connection.On<string, ChallengeId>(
            "ChallengeAcceptedAsync",
            (gameToken, challengeId) =>
                _acceptedChallengesChannel.Writer.TryWrite((gameToken, challengeId))
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
