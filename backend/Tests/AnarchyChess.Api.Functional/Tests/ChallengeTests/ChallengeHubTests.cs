using AnarchyChess.Api.Challenges.Models;
using AnarchyChess.Api.TestInfrastructure;
using AnarchyChess.Api.TestInfrastructure.Fakes;
using AnarchyChess.Api.TestInfrastructure.SignalRClients;
using FluentAssertions;

namespace AnarchyChess.Api.Functional.Tests.ChallengeTests;

public class ChallengeHubTests(AnarchyChessWebApplicationFactory factory)
    : BaseFunctionalTest(factory)
{
    [Fact]
    public async Task ChallengeReceivedAsync_is_called_for_each_challenge_when_connecting()
    {
        var recipient = new AuthedUserFaker().Generate();
        await DbContext.AddRangeAsync(recipient);
        await DbContext.SaveChangesAsync(CT);

        await AuthUtils.AuthenticateAsync(ApiClient);

        List<ChallengeRequest> createdChallenges = [];
        for (var i = 0; i < 3; i++)
        {
            await AuthUtils.AuthenticateAsync(ApiClient);
            var createResult = await ApiClient.Api.CreateChallengeAsync(
                recipient.Id,
                new PoolKeyFaker().Generate()
            );
            createResult.Content.Should().NotBeNull();
            createdChallenges.Add(createResult.Content);
        }

        await using ChallengeHubClient recipientConn = new(
            AuthedSignalR(ChallengeHubClient.Path, recipient)
        );
        await recipientConn.StartAsync(CT);

        List<ChallengeRequest> receivedChallenges = [];
        for (var i = 0; i < createdChallenges.Count; i++)
        {
            var receivedChallenge = await recipientConn.GetNextIncomingRequestAsync(CT);
            receivedChallenges.Add(receivedChallenge);
        }

        receivedChallenges.Should().BeEquivalentTo(createdChallenges);
    }
}
