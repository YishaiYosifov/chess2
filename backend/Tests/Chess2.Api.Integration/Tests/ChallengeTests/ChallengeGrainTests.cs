using Chess2.Api.Challenges.Errors;
using Chess2.Api.Challenges.Grains;
using Chess2.Api.Challenges.Models;
using Chess2.Api.Challenges.Services;
using Chess2.Api.LiveGame.Services;
using Chess2.Api.Preferences.Services;
using Chess2.Api.Profile.Entities;
using Chess2.Api.Profile.Errors;
using Chess2.Api.Profile.Models;
using Chess2.Api.Shared.Models;
using Chess2.Api.Social.Services;
using Chess2.Api.TestInfrastructure;
using Chess2.Api.TestInfrastructure.Fakes;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NSubstitute;
using Orleans.TestKit;
using Orleans.TestKit.Storage;

namespace Chess2.Api.Integration.Tests.ChallengeTests;

public class ChallengeGrainTests : BaseOrleansIntegrationTest
{
    private readonly UserId _requesterId = "requester-id";
    private readonly IChallengeInboxGrain _requesterInbox;
    private readonly AuthedUser _requester;

    private readonly UserId _recipientId = "recipient-id";
    private readonly IChallengeInboxGrain _recipientInbox;
    private readonly AuthedUser _recipient;

    private readonly ChallengeId _challengeId = "test challenge";
    private readonly DateTimeOffset _fakeNow = DateTimeOffset.UtcNow;

    private readonly IBlockService _blockService;
    private readonly ChallengeSettings _settings;

    private readonly IChallengeNotifier _challengeNotifierMock =
        Substitute.For<IChallengeNotifier>();
    private readonly TimeProvider _timeProviderMock = Substitute.For<TimeProvider>();

    private readonly ChallengeGrainStorage _state;
    private readonly TestStorageStats _stateStats;

    public ChallengeGrainTests(Chess2WebApplicationFactory factory)
        : base(factory)
    {
        _timeProviderMock.GetUtcNow().Returns(_fakeNow);
        _requester = new AuthedUserFaker().RuleFor(x => x.Id, _requesterId.Value).Generate();
        _recipient = new AuthedUserFaker().RuleFor(x => x.Id, _recipientId.Value).Generate();

        _blockService = ApiTestBase.Scope.ServiceProvider.GetRequiredService<IBlockService>();
        var settings = ApiTestBase.Scope.ServiceProvider.GetRequiredService<
            IOptions<AppSettings>
        >();
        _settings = settings.Value.Challenge;

        var grains = ApiTestBase.Scope.ServiceProvider.GetRequiredService<IGrainFactory>();
        _requesterInbox = grains.GetGrain<IChallengeInboxGrain>(_requesterId);
        _recipientInbox = grains.GetGrain<IChallengeInboxGrain>(_recipientId);

        Silo.AddProbe(id =>
        {
            if (id.ToString() == _requesterId)
                return _requesterInbox;
            else if (id.ToString() == _recipientId)
                return _recipientInbox;
            return null!;
        });

        Silo.ServiceProvider.AddService(_challengeNotifierMock);
        Silo.ServiceProvider.AddService(_timeProviderMock);
        Silo.ServiceProvider.AddService(settings);
        Silo.ServiceProvider.AddService(
            ApiTestBase.Scope.ServiceProvider.GetRequiredService<IInteractionLevelGate>()
        );
        Silo.ServiceProvider.AddService(
            ApiTestBase.Scope.ServiceProvider.GetRequiredService<UserManager<AuthedUser>>()
        );
        Silo.ServiceProvider.AddService(
            ApiTestBase.Scope.ServiceProvider.GetRequiredService<IGameStarter>()
        );

        _state = Silo
            .StorageManager.GetStorage<ChallengeGrainStorage>(ChallengeGrain.StateName)
            .State;
        _stateStats = Silo.StorageManager.GetStorageStats(ChallengeGrain.StateName)!;
    }

    private Task<ChallengeGrain> CreateGrainAsync() =>
        Silo.CreateGrainAsync<ChallengeGrain>(_challengeId);

    [Fact]
    public async Task CreateAsync_rejects_when_requester_and_recipient_is_the_same_user()
    {
        await ApiTestBase.DbContext.AddRangeAsync(_requester, _recipient);
        await ApiTestBase.DbContext.SaveChangesAsync(ApiTestBase.CT);
        var grain = await CreateGrainAsync();

        var result = await grain.CreateAsync(
            _requesterId,
            _requesterId,
            new PoolKeyFaker().Generate()
        );

        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(ChallengeErrors.CannotChallengeSelf);
    }

    [Fact]
    public async Task CreateAsync_rejects_when_recipient_is_not_found()
    {
        await ApiTestBase.DbContext.AddAsync(_requester, ApiTestBase.CT);
        await ApiTestBase.DbContext.SaveChangesAsync(ApiTestBase.CT);
        var grain = await CreateGrainAsync();

        var result = await grain.CreateAsync(
            _requesterId,
            _recipientId,
            new PoolKeyFaker().Generate()
        );

        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(ProfileErrors.NotFound);
    }

    [Fact]
    public async Task CreateAsync_rejects_when_recipient_is_not_accepting()
    {
        await ApiTestBase.DbContext.AddRangeAsync(_requester, _recipient);
        await ApiTestBase.DbContext.SaveChangesAsync(ApiTestBase.CT);
        await _blockService.BlockUserAsync(
            _recipientId,
            userIdToBlock: _requesterId,
            ApiTestBase.CT
        );
        var grain = await CreateGrainAsync();

        var result = await grain.CreateAsync(
            _requesterId,
            _recipientId,
            new PoolKeyFaker().Generate()
        );

        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(ChallengeErrors.RecipientNotAccepting);
    }

    [Fact]
    public async Task CreateAsync_sets_challenge_up_correctly()
    {
        await ApiTestBase.DbContext.AddRangeAsync(_requester, _recipient);
        await ApiTestBase.DbContext.SaveChangesAsync(ApiTestBase.CT);
        var grain = await CreateGrainAsync();

        var pool = new PoolKeyFaker().Generate();
        var result = await grain.CreateAsync(_requesterId, _recipientId, pool);

        result.IsError.Should().BeFalse();

        ChallengeRequest expectedChallenge = new(
            ChallengeId: _challengeId,
            Requester: new(_requester),
            Recipient: new(_recipient),
            Pool: pool,
            ExpiresAt: _fakeNow.DateTime + _settings.ChallengeLifetime
        );
        await _challengeNotifierMock
            .Received(1)
            .NotifyChallengeReceived(recipientId: _recipientId, expectedChallenge);

        var requesterInbox = await _requesterInbox.GetIncomingChallengesAsync();
        requesterInbox.Should().BeEmpty();

        var recipientInbox = await _recipientInbox.GetIncomingChallengesAsync();
        recipientInbox.Should().ContainSingle().Which.Should().Be(expectedChallenge);

        _state.Request.Should().Be(expectedChallenge);
        _stateStats.Writes.Should().Be(1);

        Silo.ReminderRegistry.Mock.Verify(x =>
            x.RegisterOrUpdateReminder(
                Silo.GetGrainId(grain),
                ChallengeGrain.TimeoutReminderName,
                _settings.ChallengeLifetime,
                TimeSpan.Zero
            )
        );
    }
}
