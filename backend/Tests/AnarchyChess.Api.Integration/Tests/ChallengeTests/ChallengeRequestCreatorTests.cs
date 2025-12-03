using AnarchyChess.Api.Challenges.Errors;
using AnarchyChess.Api.Challenges.Grains;
using AnarchyChess.Api.Challenges.Models;
using AnarchyChess.Api.Challenges.Services;
using AnarchyChess.Api.Matchmaking.Models;
using AnarchyChess.Api.Preferences.Services;
using AnarchyChess.Api.Profile.DTOs;
using AnarchyChess.Api.Profile.Entities;
using AnarchyChess.Api.Profile.Models;
using AnarchyChess.Api.Shared.Models;
using AnarchyChess.Api.Shared.Services;
using AnarchyChess.Api.Social.Services;
using AnarchyChess.Api.TestInfrastructure;
using AnarchyChess.Api.TestInfrastructure.Fakes;
using AwesomeAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace AnarchyChess.Api.Integration.Tests.ChallengeTests;

public class ChallengeRequestCreatorTests : BaseIntegrationTest
{
    private readonly ChallengeRequestCreator _challengeCreator;

    private readonly ChallengeSettings _settings;
    private readonly IBlockService _blockService;
    private readonly IGrainFactory _grains;

    private readonly DateTimeOffset _fakeNow = DateTimeOffset.UtcNow;
    private readonly TimeProvider _timeProviderMock = Substitute.For<TimeProvider>();

    public ChallengeRequestCreatorTests(AnarchyChessWebApplicationFactory factory)
        : base(factory)
    {
        _grains = Scope.ServiceProvider.GetRequiredService<IGrainFactory>();
        _blockService = Scope.ServiceProvider.GetRequiredService<IBlockService>();

        var settings = Scope.ServiceProvider.GetRequiredService<IOptions<AppSettings>>();
        _settings = settings.Value.Challenge;

        _timeProviderMock.GetUtcNow().Returns(_fakeNow);

        _challengeCreator = new(
            _grains,
            settings,
            Scope.ServiceProvider.GetRequiredService<IRandomCodeGenerator>(),
            Scope.ServiceProvider.GetRequiredService<IInteractionLevelGate>(),
            _timeProviderMock,
            Scope.ServiceProvider.GetRequiredService<UserManager<AuthedUser>>()
        );
    }

    [Fact]
    public async Task CreateAsync_returns_expected_open_challenge_when_no_recipient()
    {
        var requester = new AuthedUserFaker().Generate();
        await DbContext.AddAsync(requester, CT);
        await DbContext.SaveChangesAsync(CT);

        var pool = new PoolKeyFaker().Generate();
        var result = await _challengeCreator.CreateAsync(
            requesterId: requester.Id,
            recipientId: null,
            pool: pool
        );

        result.IsError.Should().BeFalse();
        var challenge = result.Value;
        challenge
            .Should()
            .BeEquivalentTo(
                new ChallengeRequest(
                    ChallengeToken: challenge.ChallengeToken,
                    Requester: new MinimalProfile(requester),
                    Recipient: null,
                    Pool: pool,
                    ExpiresAt: _fakeNow.UtcDateTime + _settings.ChallengeLifetime
                )
            );
        challenge.ChallengeToken.Value.Should().HaveLength(16);
    }

    [Fact]
    public async Task CreateAsync_returns_expected_challenge_when_recipient_specified()
    {
        var requester = new AuthedUserFaker().Generate();
        var recipient = new AuthedUserFaker().Generate();

        await DbContext.AddRangeAsync(requester, recipient);
        await DbContext.SaveChangesAsync(CT);

        var pool = new PoolKeyFaker().Generate();
        var result = await _challengeCreator.CreateAsync(
            requester.Id,
            recipientId: recipient.Id,
            pool: pool
        );

        result.IsError.Should().BeFalse();
        var challenge = result.Value;
        challenge
            .Should()
            .BeEquivalentTo(
                new ChallengeRequest(
                    ChallengeToken: challenge.ChallengeToken,
                    Requester: new MinimalProfile(requester),
                    Recipient: new MinimalProfile(recipient),
                    Pool: pool,
                    ExpiresAt: _fakeNow.UtcDateTime + _settings.ChallengeLifetime
                )
            );
        challenge.ChallengeToken.Value.Should().HaveLength(16);
    }

    [Fact]
    public async Task CreateAsync_rejects_when_requester_and_recipient_is_the_same_user()
    {
        var requester = new AuthedUserFaker().Generate();
        await DbContext.AddAsync(requester, CT);
        await DbContext.SaveChangesAsync(CT);

        var result = await _challengeCreator.CreateAsync(
            requester.Id,
            recipientId: requester.Id,
            pool: new PoolKeyFaker().Generate()
        );

        result.IsError.Should().BeTrue();
        result.Errors.First().Should().Be(ChallengeErrors.CannotChallengeSelf);
    }

    [Fact]
    public async Task CreateAsync_rejects_guests_creating_rated_challenge()
    {
        var result = await _challengeCreator.CreateAsync(
            requesterId: UserId.Guest(),
            recipientId: null,
            pool: new PoolKeyFaker().RuleFor(x => x.PoolType, PoolType.Rated)
        );

        result.IsError.Should().BeTrue();
        result.Errors.First().Should().Be(ChallengeErrors.AuthedOnlyPool);
    }

    [Fact]
    public async Task CreateAsync_rejects_when_recipient_not_accepting()
    {
        var requester = new AuthedUserFaker().Generate();
        var recipient = new AuthedUserFaker().Generate();
        await DbContext.AddRangeAsync(requester, recipient);
        await DbContext.SaveChangesAsync(CT);

        await _blockService.BlockUserAsync(recipient.Id, requester.Id, CT);

        var result = await _challengeCreator.CreateAsync(
            requester.Id,
            recipientId: recipient.Id,
            pool: new PoolKeyFaker().Generate()
        );

        result.IsError.Should().BeTrue();
        result.Errors.First().Should().Be(ChallengeErrors.RecipientNotAccepting);
    }

    [Fact]
    public async Task CreateAsync_rejects_when_the_requester_already_sent_the_recipient_a_challenge()
    {
        var requester = new AuthedUserFaker().Generate();
        var recipient = new AuthedUserFaker().Generate();
        await DbContext.AddRangeAsync(requester, recipient);
        await DbContext.SaveChangesAsync(CT);

        var result1 = await _challengeCreator.CreateAsync(
            requester.Id,
            recipient.Id,
            pool: new PoolKeyFaker().Generate()
        );
        result1.IsError.Should().BeFalse();
        await _grains
            .GetGrain<IChallengeInboxGrain>(recipient.Id)
            .RecordChallengeCreatedAsync(result1.Value);

        var result2 = await _challengeCreator.CreateAsync(
            requester.Id,
            recipientId: recipient.Id,
            pool: new PoolKeyFaker().Generate()
        );

        result2.IsError.Should().BeTrue();
        result2.Errors.First().Should().Be(ChallengeErrors.AlreadyExists);
    }

    [Fact]
    public async Task CreateAsync_allows_when_the_recipient_already_has_a_request_from_another_requester()
    {
        var anotherRequester = new AuthedUserFaker().Generate();
        var requester = new AuthedUserFaker().Generate();
        var recipient = new AuthedUserFaker().Generate();
        await DbContext.AddRangeAsync(requester, recipient, anotherRequester);
        await DbContext.SaveChangesAsync(CT);

        await _challengeCreator.CreateAsync(
            anotherRequester.Id,
            recipient.Id,
            pool: new PoolKeyFaker().Generate()
        );

        var result = await _challengeCreator.CreateAsync(
            requester.Id,
            recipient.Id,
            pool: new PoolKeyFaker().Generate()
        );

        result.IsError.Should().BeFalse();
    }
}
