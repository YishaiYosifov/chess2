using System.Threading.Channels;
using AnarchyChess.Api.Challenges.Models;
using AnarchyChess.Api.Game.Models;
using AnarchyChess.Api.Profile.Models;
using AnarchyChess.Api.TestInfrastructure.Utils;
using Microsoft.AspNetCore.SignalR.Client;

namespace AnarchyChess.Api.TestInfrastructure.SignalRClients;

public class ChallengeInstanceHubClient : BaseHubClient
{
    public static string Path(ChallengeToken challengeToken) =>
        $"/api/hub/challenge?challengeToken={challengeToken}";

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

    public ChallengeInstanceHubClient(HubConnection connection)
        : base(connection)
    {
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

    public async Task<(
        UserId CancelledBy,
        ChallengeToken ChallengeToken
    )> GetNextCancelledChallengeAsync(CancellationToken token = default)
    {
        using var cts = token.WithTimeout(TimeSpan.FromSeconds(10));
        var challengeToken = await _cancelledChallengesChannel.Reader.ReadAsync(cts.Token);
        return challengeToken;
    }

    public async Task<(
        GameToken GameToken,
        ChallengeToken ChallengeToken
    )> GetNextAcceptedChallengeAsync(CancellationToken token = default)
    {
        using var cts = token.WithTimeout(TimeSpan.FromSeconds(10));
        var accepted = await _acceptedChallengesChannel.Reader.ReadAsync(cts.Token);
        return accepted;
    }
}
