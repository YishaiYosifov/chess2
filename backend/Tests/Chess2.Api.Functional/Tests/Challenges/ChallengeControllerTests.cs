using System.Net;
using Chess2.Api.TestInfrastructure;
using Chess2.Api.TestInfrastructure.Fakes;
using Chess2.Api.TestInfrastructure.SignalRClients;
using FluentAssertions;

namespace Chess2.Api.Functional.Tests.Challenges;

public class ChallengeControllerTests(Chess2WebApplicationFactory factory)
    : BaseFunctionalTest(factory)
{
    [Fact]
    public async Task CreateChallenge_creates_challenge_successfully()
    {
        var requester = new AuthedUserFaker().Generate();
        var recipient = new AuthedUserFaker().Generate();
        await DbContext.AddRangeAsync(requester, recipient);
        await DbContext.SaveChangesAsync(CT);

        await using ChallengeHubClient recipientConn = new(
            await AuthedSignalRAsync(ChallengeHubClient.Path, recipient)
        );
        await AuthUtils.AuthenticateWithUserAsync(ApiClient, requester);
        var pool = new PoolKeyFaker().Generate();

        var result = await ApiClient.Api.CreateChallengeAsync(recipient.Id, pool);

        result.IsSuccessful.Should().BeTrue();

        var recipientIncomingChallenge = await recipientConn.GetNextIncomingRequestAsync(CT);
        recipientIncomingChallenge.Requester.UserId.Should().Be(requester.Id);
        recipientIncomingChallenge.Recipient.UserId.Should().Be(recipient.Id);
        recipientIncomingChallenge.Pool.Should().Be(pool);
    }

    [Fact]
    public async Task CreateChallenge_rejects_unauthorized()
    {
        var recipient = new AuthedUserFaker().Generate();
        await DbContext.AddAsync(recipient, CT);
        await DbContext.SaveChangesAsync(CT);
        AuthUtils.AuthenticateGuest(ApiClient, "test guest");

        var result = await ApiClient.Api.CreateChallengeAsync(
            recipient.Id,
            new PoolKeyFaker().Generate()
        );

        result.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
