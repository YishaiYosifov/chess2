using System.Threading.Channels;
using Chess2.Api.Challenges.Models;
using Microsoft.AspNetCore.SignalR.Client;

namespace Chess2.Api.TestInfrastructure.SignalRClients;

public class ChallengeHubClient : BaseHubClient
{
    public const string Path = "/api/hub/challenge";

    private readonly Channel<ChallengeRequest> _incomingChallengesChannel =
        Channel.CreateUnbounded<ChallengeRequest>();
    private readonly Channel<ChallengeId> _cancelledChallengesChannel =
        Channel.CreateUnbounded<ChallengeId>();
    private readonly Channel<(
        string GameToken,
        ChallengeId ChallengeId
    )> _acceptedChallengesChannel = Channel.CreateUnbounded<(
        string GameToken,
        ChallengeId ChallengeId
    )>();

    public ChallengeHubClient(HubConnection connection)
        : base(connection)
    {
        connection.On<ChallengeRequest>(
            "ChallengeReceivedAsync",
            challenge => _incomingChallengesChannel.Writer.TryWrite(challenge)
        );
        connection.On<ChallengeId>(
            "ChallengeCancelledAsync",
            challengeId => _cancelledChallengesChannel.Writer.TryWrite(challengeId)
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
        TimeSpan timeout = TimeSpan.FromSeconds(10);
        using var timeoutCts = new CancellationTokenSource(timeout);
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
            token,
            timeoutCts.Token
        );

        var challenge = await _incomingChallengesChannel.Reader.ReadAsync(linkedCts.Token);
        return challenge;
    }

    public async Task<ChallengeId> GetNextCancelledChallengeAsync(CancellationToken token = default)
    {
        TimeSpan timeout = TimeSpan.FromSeconds(10);
        using var timeoutCts = new CancellationTokenSource(timeout);
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
            token,
            timeoutCts.Token
        );

        var challengeId = await _cancelledChallengesChannel.Reader.ReadAsync(linkedCts.Token);
        return challengeId;
    }

    public async Task<(string GameToken, ChallengeId ChallengeId)> GetNextAcceptedChallengeAsync(
        CancellationToken token = default
    )
    {
        TimeSpan timeout = TimeSpan.FromSeconds(10);
        using var timeoutCts = new CancellationTokenSource(timeout);
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
            token,
            timeoutCts.Token
        );

        var accepted = await _acceptedChallengesChannel.Reader.ReadAsync(linkedCts.Token);
        return accepted;
    }
}
