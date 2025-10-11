using Chess2.Api.Challenges.Models;
using Chess2.Api.LiveGame.Models;
using Chess2.Api.Profile.Models;
using Chess2.Api.TestInfrastructure.Utils;
using Microsoft.AspNetCore.SignalR.Client;
using System.Threading.Channels;

namespace Chess2.Api.TestInfrastructure.SignalRClients;

public class ChallengeInstanceHubClient : BaseHubClient
{
    public static string Path(ChallengeId challengeId) =>
        $"/api/hub/challenge?challengeId={challengeId}";

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

    public ChallengeInstanceHubClient(HubConnection connection)
        : base(connection)
    {
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

    public async Task<(UserId CancelledBy, ChallengeId ChallengeId)> GetNextCancelledChallengeAsync(
        CancellationToken token = default
    )
    {
        using var cts = token.WithTimeout(TimeSpan.FromSeconds(10));
        var challengeId = await _cancelledChallengesChannel.Reader.ReadAsync(cts.Token);
        return challengeId;
    }

    public async Task<(GameToken GameToken, ChallengeId ChallengeId)> GetNextAcceptedChallengeAsync(
        CancellationToken token = default
    )
    {
        using var cts = token.WithTimeout(TimeSpan.FromSeconds(10));
        var accepted = await _acceptedChallengesChannel.Reader.ReadAsync(cts.Token);
        return accepted;
    }
}
