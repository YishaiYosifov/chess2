using System.Net;
using Chess2.Api.Challenges.Models;
using Chess2.Api.Profile.Entities;
using Chess2.Api.TestInfrastructure;
using Chess2.Api.TestInfrastructure.Fakes;
using Chess2.Api.TestInfrastructure.SignalRClients;
using FluentAssertions;

namespace Chess2.Api.Functional.Tests.ChallengeTests;

public class ChallengeControllerTests(Chess2WebApplicationFactory factory)
    : BaseFunctionalTest(factory)
{
    [Fact]
    public async Task CreateChallenge_creates_challenge_successfully()
    {
        var requester = new AuthedUserFaker().Generate();
        var recipient = new AuthedUserFaker().Generate();

        await using ChallengeHubClient recipientConn = new(
            await AuthedSignalRAsync(ChallengeHubClient.Path, recipient)
        );

        var challenge = await CreateChallengeAsync(requester, recipient);

        var recipientIncomingChallenge = await recipientConn.GetNextIncomingRequestAsync(CT);
        recipientIncomingChallenge.Requester.UserId.Should().Be(requester.Id);
        recipientIncomingChallenge.Recipient.UserId.Should().Be(recipient.Id);
        recipientIncomingChallenge.Pool.Should().Be(challenge.Pool);
    }

    [Fact]
    public async Task CreateChallenge_creates_a_unique_id()
    {
        var challenge1 = await CreateChallengeAsync();
        var challenge2 = await CreateChallengeAsync();

        challenge1.ChallengeId.Should().NotBe(challenge2.ChallengeId);
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

    [Fact]
    public async Task GetChallenge_returns_correct_challenge()
    {
        var requester = new AuthedUserFaker().Generate();
        var challenge = await CreateChallengeAsync(requester);
        await AuthUtils.AuthenticateWithUserAsync(ApiClient, requester);

        var result = await ApiClient.Api.GetChallengeAsync(challenge.ChallengeId);

        result.IsSuccessful.Should().BeTrue();
        result.Content.Should().NotBeNull();
        result.Content.Should().BeEquivalentTo(challenge);
    }

    [Fact]
    public async Task GetChallenge_rejects_unauthorized()
    {
        var challenge = await CreateChallengeAsync();
        AuthUtils.AuthenticateGuest(ApiClient, "test guest");

        var result = await ApiClient.Api.GetChallengeAsync(challenge.ChallengeId);

        result.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CancelChallenge_cancels_challenge_successfully()
    {
        var recipient = new AuthedUserFaker().Generate();
        var challenge = await CreateChallengeAsync(recipient: recipient);

        await using ChallengeHubClient recipientConn = new(
            await AuthedSignalRAsync(ChallengeHubClient.Path, recipient)
        );

        await AuthUtils.AuthenticateWithUserAsync(ApiClient, recipient);
        var cancelResult = await ApiClient.Api.CancelChallengeAsync(challenge.ChallengeId);

        cancelResult.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var cancelledId = await recipientConn.GetNextCancelledChallengeAsync(CT);
        cancelledId.Should().Be(challenge.ChallengeId);
    }

    [Fact]
    public async Task CancelChallenge_rejects_unauthorized()
    {
        AuthUtils.AuthenticateGuest(ApiClient, "test guest");

        var result = await ApiClient.Api.CancelChallengeAsync("challenge id");

        result.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task AcceptChallenge_accepts_challenge_successfully()
    {
        var requester = new AuthedUserFaker().Generate();
        var recipient = new AuthedUserFaker().Generate();

        var challenge = await CreateChallengeAsync(requester, recipient);

        await using ChallengeHubClient requesterConn = new(
            await AuthedSignalRAsync(ChallengeHubClient.Path, requester)
        );

        await AuthUtils.AuthenticateWithUserAsync(ApiClient, recipient);
        var acceptResult = await ApiClient.Api.AcceptChallengeAsync(challenge.ChallengeId);

        acceptResult.IsSuccessful.Should().BeTrue();
        acceptResult.Content.Should().NotBeNull();

        var accepted = await requesterConn.GetNextAcceptedChallengeAsync(CT);
        accepted.ChallengeId.Should().Be(challenge.ChallengeId);
        accepted.GameToken.Should().Be(acceptResult.Content);

        var createdGameStateResult = await ApiClient.Api.GetGameAsync(accepted.GameToken);
        createdGameStateResult.IsSuccessful.Should().BeTrue();
        createdGameStateResult.Content.Should().NotBeNull();
        createdGameStateResult.Content.Pool.Should().Be(challenge.Pool);
        string[] playerIds =
        [
            createdGameStateResult.Content.WhitePlayer.UserId,
            createdGameStateResult.Content.BlackPlayer.UserId,
        ];
        playerIds.Should().BeEquivalentTo([requester.Id, recipient.Id]);
    }

    [Fact]
    public async Task AcceptChallenge_rejects_unauthorized()
    {
        AuthUtils.AuthenticateGuest(ApiClient, "test guest");

        var result = await ApiClient.Api.AcceptChallengeAsync("challenge id");

        result.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    private async Task<ChallengeRequest> CreateChallengeAsync(
        AuthedUser? requester = null,
        AuthedUser? recipient = null
    )
    {
        requester ??= new AuthedUserFaker().Generate();
        recipient ??= new AuthedUserFaker().Generate();
        await DbContext.AddRangeAsync(requester, recipient);
        await DbContext.SaveChangesAsync(CT);

        await AuthUtils.AuthenticateWithUserAsync(ApiClient, requester);
        var pool = new PoolKeyFaker().Generate();
        var result = await ApiClient.Api.CreateChallengeAsync(recipient.Id, pool);

        result.IsSuccessful.Should().BeTrue();
        result.Content.Should().NotBeNull();

        return result.Content;
    }
}
