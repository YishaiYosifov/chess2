using System.Net;
using Chess2.Api.Challenges.Models;
using Chess2.Api.Profile.Entities;
using Chess2.Api.Profile.Models;
using Chess2.Api.TestInfrastructure;
using Chess2.Api.TestInfrastructure.Fakes;
using Chess2.Api.TestInfrastructure.SignalRClients;
using FluentAssertions;

namespace Chess2.Api.Functional.Tests.ChallengeTests;

public class ChallengeControllerTests(Chess2WebApplicationFactory factory)
    : BaseFunctionalTest(factory)
{
    private readonly AuthedUser _requester = new AuthedUserFaker().Generate();
    private readonly AuthedUser _recipient = new AuthedUserFaker().Generate();

    [Fact]
    public async Task CreateChallenge_creates_challenge_successfully()
    {
        await using ChallengeHubClient recipientConn = new(
            await AuthedSignalRAsync(ChallengeHubClient.Path, _recipient)
        );

        var challenge = await CreateChallengeAsync(_requester, _recipient);

        var recipientIncomingChallenge = await recipientConn.GetNextIncomingRequestAsync(CT);
        recipientIncomingChallenge.Requester.UserId.Should().Be((UserId)_requester.Id);
        recipientIncomingChallenge.Recipient?.UserId.Should().Be((UserId)_recipient.Id);
        recipientIncomingChallenge.Pool.Should().Be(challenge.Pool);
    }

    [Fact]
    public async Task CreateChallenge_rejects_guest_with_recipient()
    {
        AuthUtils.AuthenticateGuest(ApiClient, "guest id");

        var result = await ApiClient.Api.CreateChallengeAsync(
            "some-recipient-id",
            new PoolKeyFaker().Generate()
        );

        result.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CreateChallenge_allows_guest_with_null_recipient()
    {
        AuthUtils.AuthenticateGuest(ApiClient, "guest id");

        var pool = new PoolKeyFaker().Generate();
        var result = await ApiClient.Api.CreateChallengeAsync(null, pool);

        result.IsSuccessful.Should().BeTrue();
        result.Content.Should().NotBeNull();
        result.Content.Requester.UserId.Should().Be((UserId)"guest id");
        result.Content.Recipient.Should().BeNull();
        result.Content.Pool.Should().Be(pool);
    }

    [Fact]
    public async Task CreateChallenge_allows_null_recipient()
    {
        var challenge = await CreateChallengeAsync(_requester);
        challenge.Requester.UserId.Should().Be((UserId)_requester.Id);
        challenge.Recipient.Should().BeNull();
    }

    [Fact]
    public async Task CreateChallenge_creates_a_unique_id()
    {
        var challenge1 = await CreateChallengeAsync();
        var challenge2 = await CreateChallengeAsync();

        challenge1.ChallengeId.Should().NotBe(challenge2.ChallengeId);
    }

    [Fact]
    public async Task GetChallenge_returns_correct_challenge()
    {
        var challenge = await CreateChallengeAsync(_requester, _recipient);
        await AuthUtils.AuthenticateWithUserAsync(ApiClient, _requester);

        var result = await ApiClient.Api.GetChallengeAsync(challenge.ChallengeId);

        result.IsSuccessful.Should().BeTrue();
        result.Content.Should().NotBeNull();
        result.Content.Should().BeEquivalentTo(challenge);
    }

    [Fact]
    public async Task CancelChallenge_cancels_challenge_successfully()
    {
        var challenge = await CreateChallengeAsync(_requester, _recipient);

        await using ChallengeHubClient recipientConn = new(
            await AuthedSignalRAsync(ChallengeHubClient.Path, _recipient)
        );

        await AuthUtils.AuthenticateWithUserAsync(ApiClient, _recipient);
        var cancelResult = await ApiClient.Api.CancelChallengeAsync(challenge.ChallengeId);

        cancelResult.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var cancelledId = await recipientConn.GetNextCancelledChallengeAsync(CT);
        cancelledId.Should().Be(challenge.ChallengeId);
    }

    [Fact]
    public async Task AcceptChallenge_accepts_challenge_successfully()
    {
        var challenge = await CreateChallengeAsync(_requester, _recipient);

        await using ChallengeHubClient requesterConn = new(
            await AuthedSignalRAsync(ChallengeHubClient.Path, _requester)
        );

        await AuthUtils.AuthenticateWithUserAsync(ApiClient, _recipient);
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
        playerIds.Should().BeEquivalentTo([_requester.Id, _recipient.Id]);
    }

    private async Task<ChallengeRequest> CreateChallengeAsync(
        AuthedUser? requester = null,
        AuthedUser? recipient = null
    )
    {
        requester ??= new AuthedUserFaker().Generate();
        await DbContext.AddAsync(requester, CT);
        if (recipient is not null)
            await DbContext.AddAsync(recipient, CT);
        await DbContext.SaveChangesAsync(CT);

        await AuthUtils.AuthenticateWithUserAsync(ApiClient, requester);
        var result = await ApiClient.Api.CreateChallengeAsync(
            recipient?.Id,
            new PoolKeyFaker().Generate()
        );

        result.IsSuccessful.Should().BeTrue();
        result.Content.Should().NotBeNull();

        return result.Content;
    }
}
