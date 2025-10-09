using Chess2.Api.Challenges.Errors;
using Chess2.Api.Challenges.Grains;
using Chess2.Api.Challenges.Models;
using Chess2.Api.Challenges.Services;
using Chess2.Api.GameSnapshot.Services;
using Chess2.Api.Matchmaking.Models;
using Chess2.Api.Preferences.Services;
using Chess2.Api.Profile.DTOs;
using Chess2.Api.Profile.Entities;
using Chess2.Api.Profile.Models;
using Chess2.Api.Shared.Models;
using Chess2.Api.Shared.Services;
using Chess2.Api.Social.Services;
using Chess2.Api.TestInfrastructure;
using Chess2.Api.TestInfrastructure.Fakes;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace Chess2.Api.Integration.Tests.ChallengeTests;

public class ChallengeRequestCreatorTests : BaseIntegrationTest
{
    private readonly ChallengeRequestCreator _challengeCreator;

    private readonly ChallengeSettings _settings;
    private readonly ITimeControlTranslator _timeControlTranslator;
    private readonly IBlockService _blockService;
    private readonly IGrainFactory _grains;

    private readonly DateTimeOffset _fakeNow = DateTimeOffset.UtcNow;
    private readonly TimeProvider _timeProviderMock = Substitute.For<TimeProvider>();

    public ChallengeRequestCreatorTests(Chess2WebApplicationFactory factory)
        : base(factory)
    {
        _grains = Scope.ServiceProvider.GetRequiredService<IGrainFactory>();
        _blockService = Scope.ServiceProvider.GetRequiredService<IBlockService>();
        _timeControlTranslator = Scope.ServiceProvider.GetRequiredService<ITimeControlTranslator>();

        var settings = Scope.ServiceProvider.GetRequiredService<IOptions<AppSettings>>();
        _settings = settings.Value.Challenge;

        _timeProviderMock.GetUtcNow().Returns(_fakeNow);

        _challengeCreator = new(
            _grains,
            settings,
            Scope.ServiceProvider.GetRequiredService<IRandomCodeGenerator>(),
            Scope.ServiceProvider.GetRequiredService<ITimeControlTranslator>(),
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
                    ChallengeId: challenge.ChallengeId,
                    Requester: new MinimalProfile(requester),
                    Recipient: null,
                    Pool: pool,
                    TimeControl: _timeControlTranslator.FromSeconds(pool.TimeControl.BaseSeconds),
                    ExpiresAt: _fakeNow.UtcDateTime + _settings.ChallengeLifetime
                )
            );
        challenge.ChallengeId.Value.Should().HaveLength(16);
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
                    ChallengeId: challenge.ChallengeId,
                    Requester: new MinimalProfile(requester),
                    Recipient: new MinimalProfile(recipient),
                    Pool: pool,
                    TimeControl: _timeControlTranslator.FromSeconds(pool.TimeControl.BaseSeconds),
                    ExpiresAt: _fakeNow.UtcDateTime + _settings.ChallengeLifetime
                )
            );
        challenge.ChallengeId.Value.Should().HaveLength(16);
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
