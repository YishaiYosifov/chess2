using System.Net;
using AnarchyChess.Api.Challenges.Models;
using AnarchyChess.Api.Game.Models;
using AnarchyChess.Api.GameSnapshot.Models;
using AnarchyChess.Api.Matchmaking.Models;
using AnarchyChess.Api.Profile.Entities;
using AnarchyChess.Api.Profile.Models;
using AnarchyChess.Api.TestInfrastructure;
using AnarchyChess.Api.TestInfrastructure.Fakes;
using AnarchyChess.Api.TestInfrastructure.SignalRClients;
using FluentAssertions;

namespace AnarchyChess.Api.Functional.Tests.ChallengeTests;

public class ChallengeControllerTests(AnarchyChessWebApplicationFactory factory)
    : BaseFunctionalTest(factory)
{
    private readonly AuthedUser _requester = new AuthedUserFaker().Generate();
    private readonly AuthedUser _recipient = new AuthedUserFaker().Generate();

    [Fact]
    public async Task CreateChallenge_creates_challenge_successfully()
    {
        await DbContext.AddRangeAsync(_requester, _recipient);
        await DbContext.SaveChangesAsync(CT);
        await using ChallengeHubClient recipientConn = new(
            await AuthedSignalRAsync(ChallengeHubClient.Path, _recipient)
        );

        var challenge = await CreateChallengeAsync(_requester, _recipient);

        var recipientIncomingChallenge = await recipientConn.GetNextIncomingRequestAsync(CT);
        recipientIncomingChallenge.Requester.UserId.Should().Be(_requester.Id);
        recipientIncomingChallenge.Recipient?.UserId.Should().Be(_recipient.Id);
        recipientIncomingChallenge.Pool.Should().Be(challenge.Pool);
    }

    [Fact]
    public async Task CreateChallenge_allows_guest_with_casual_pool()
    {
        var guestId = UserId.Guest();
        AuthUtils.AuthenticateGuest(ApiClient, guestId);

        var pool = new PoolKeyFaker().RuleFor(x => x.PoolType, PoolType.Casual).Generate();
        var result = await ApiClient.Api.CreateChallengeAsync(null, pool);

        result.IsSuccessful.Should().BeTrue();
        result.Content.Should().NotBeNull();
        result.Content.Requester.UserId.Should().Be(guestId);
        result.Content.Recipient.Should().BeNull();
        result.Content.Pool.Should().Be(pool);
    }

    [Fact]
    public async Task CreateChallenge_rejects_guest_with_rated_pool()
    {
        AuthUtils.AuthenticateGuest(ApiClient);

        var result = await ApiClient.Api.CreateChallengeAsync(
            null,
            new PoolKeyFaker().RuleFor(x => x.PoolType, PoolType.Rated).Generate()
        );

        result.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CreateChallenge_allows_null_recipient_for_open_challenges()
    {
        await DbContext.AddAsync(_requester, CT);
        await DbContext.SaveChangesAsync(CT);

        var challenge = await CreateChallengeAsync(_requester);
        challenge.Requester.UserId.Should().Be(_requester.Id);
        challenge.Recipient.Should().BeNull();
    }

    [Fact]
    public async Task CreateChallenge_rejects_invalid_time_control()
    {
        AuthUtils.AuthenticateGuest(ApiClient);

        var result = await ApiClient.Api.CreateChallengeAsync(
            null,
            new PoolKeyFaker(PoolType.Casual)
                .RuleFor(x => x.TimeControl, new TimeControlSettings(1234, 55))
                .Generate()
        );

        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetChallenge_returns_correct_challenge()
    {
        await DbContext.AddRangeAsync(_requester, _recipient);
        await DbContext.SaveChangesAsync(CT);

        var challenge = await CreateChallengeAsync(_requester, _recipient);
        await AuthUtils.AuthenticateWithUserAsync(ApiClient, _requester);

        var result = await ApiClient.Api.GetChallengeAsync(challenge.ChallengeToken);

        result.IsSuccessful.Should().BeTrue();
        result.Content.Should().NotBeNull();
        result.Content.Should().BeEquivalentTo(challenge);
    }

    [Fact]
    public async Task CancelChallenge_cancels_challenge_successfully()
    {
        await DbContext.AddRangeAsync(_requester, _recipient);
        await DbContext.SaveChangesAsync(CT);

        var challenge = await CreateChallengeAsync(_requester, _recipient);

        await using ChallengeInstanceHubClient recipientConn = new(
            await AuthedSignalRAsync(
                ChallengeInstanceHubClient.Path(challenge.ChallengeToken),
                _recipient
            )
        );

        await AuthUtils.AuthenticateWithUserAsync(ApiClient, _recipient);
        var cancelResult = await ApiClient.Api.CancelChallengeAsync(challenge.ChallengeToken);

        cancelResult.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var (cancelledBy, cancelledChallengeToken) =
            await recipientConn.GetNextCancelledChallengeAsync(CT);
        cancelledBy.Should().Be(_recipient.Id);
        cancelledChallengeToken.Should().Be(challenge.ChallengeToken);
    }

    [Fact]
    public async Task AcceptChallenge_accepts_challenge_successfully()
    {
        await DbContext.AddRangeAsync(_requester, _recipient);
        await DbContext.SaveChangesAsync(CT);

        var challenge = await CreateChallengeAsync(_requester, _recipient);

        await using ChallengeInstanceHubClient requesterConn = new(
            await AuthedSignalRAsync(
                ChallengeInstanceHubClient.Path(challenge.ChallengeToken),
                _requester
            )
        );

        await AuthUtils.AuthenticateWithUserAsync(ApiClient, _recipient);
        var result = await ApiClient.Api.AcceptChallengeAsync(challenge.ChallengeToken);

        result.IsSuccessful.Should().BeTrue();
        result.Content.Should().NotBeNull();

        var (acceptedGameToken, acceptedChallengeToken) =
            await requesterConn.GetNextAcceptedChallengeAsync(CT);
        acceptedGameToken.Should().Be((GameToken)result.Content);
        acceptedChallengeToken.Should().Be(challenge.ChallengeToken);

        var createdGameStateResult = await ApiClient.Api.GetGameAsync(acceptedGameToken);
        createdGameStateResult.IsSuccessful.Should().BeTrue();
        createdGameStateResult.Content.Should().NotBeNull();
        createdGameStateResult.Content.Pool.Should().Be(challenge.Pool);
        UserId[] playerIds =
        [
            createdGameStateResult.Content.WhitePlayer.UserId,
            createdGameStateResult.Content.BlackPlayer.UserId,
        ];
        playerIds.Should().BeEquivalentTo([_requester.Id, _recipient.Id]);
    }

    [Fact]
    public async Task AcceptChallenge_rejects_guests_accepting_rated_challenges()
    {
        await DbContext.AddAsync(_requester, CT);
        await DbContext.SaveChangesAsync(CT);

        var challenge = await CreateChallengeAsync(
            _requester,
            pool: new PoolKeyFaker().RuleFor(x => x.PoolType, PoolType.Rated)
        );

        AuthUtils.AuthenticateGuest(ApiClient);
        var result = await ApiClient.Api.AcceptChallengeAsync(challenge.ChallengeToken);

        result.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CancelAllIncomingChallenges_cancels_all_incoming_challenges_for_user()
    {
        var requester1 = new AuthedUserFaker().Generate();
        var requester2 = new AuthedUserFaker().Generate();
        var recipient = new AuthedUserFaker().Generate();
        await DbContext.AddRangeAsync(requester1, requester2, recipient);
        await DbContext.SaveChangesAsync(CT);

        await AuthUtils.AuthenticateWithUserAsync(ApiClient, requester1);
        var challenge1 = await CreateChallengeAsync(requester1, recipient);
        var challenge2 = await CreateChallengeAsync(requester2, recipient);

        await AuthUtils.AuthenticateWithUserAsync(ApiClient, recipient);
        var cancelResponse = await ApiClient.Api.CancelAllIncomingChallengesAsync();

        cancelResponse.IsSuccessful.Should().BeTrue();

        var get1 = await ApiClient.Api.GetChallengeAsync(challenge1.ChallengeToken);
        get1.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var get2 = await ApiClient.Api.GetChallengeAsync(challenge2.ChallengeToken);
        get2.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private async Task<ChallengeRequest> CreateChallengeAsync(
        AuthedUser requester,
        AuthedUser? recipient = null,
        PoolKey? pool = null
    )
    {
        await AuthUtils.AuthenticateWithUserAsync(ApiClient, requester);
        var result = await ApiClient.Api.CreateChallengeAsync(
            recipient?.Id,
            pool ?? new PoolKeyFaker().Generate()
        );

        result.IsSuccessful.Should().BeTrue();
        result.Content.Should().NotBeNull();

        return result.Content;
    }
}
