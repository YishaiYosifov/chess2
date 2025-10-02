using Chess2.Api.Challenges.Errors;
using Chess2.Api.Challenges.Grains;
using Chess2.Api.Challenges.Models;
using Chess2.Api.Challenges.Services;
using Chess2.Api.LiveGame.Grains;
using Chess2.Api.LiveGame.Services;
using Chess2.Api.Matchmaking.Models;
using Chess2.Api.Preferences.Services;
using Chess2.Api.Profile.Entities;
using Chess2.Api.Profile.Errors;
using Chess2.Api.Shared.Models;
using Chess2.Api.Social.Services;
using Chess2.Api.TestInfrastructure;
using Chess2.Api.TestInfrastructure.Fakes;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using NSubstitute;
using Orleans.TestKit;
using Orleans.TestKit.Storage;

namespace Chess2.Api.Integration.Tests.ChallengeTests;

public class ChallengeGrainTests : BaseOrleansIntegrationTest
{
    private const string _requesterId = "requester-id";
    private readonly ChallengeInboxGrain _requesterInbox = new();
    private readonly AuthedUser _requester;

    private const string _recipientId = "recipient-id";
    private readonly ChallengeInboxGrain _recipientInbox = new();
    private readonly AuthedUser _recipient;

    private readonly ChallengeId _challengeId = "test challenge";
    private readonly DateTimeOffset _fakeNow = DateTimeOffset.UtcNow;

    private readonly IGrainFactory _grainFactory;
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
        _requester = new AuthedUserFaker().RuleFor(x => x.Id, _requesterId).Generate();
        _recipient = new AuthedUserFaker().RuleFor(x => x.Id, _recipientId).Generate();

        _grainFactory = ApiTestBase.Scope.ServiceProvider.GetRequiredService<IGrainFactory>();
        _blockService = ApiTestBase.Scope.ServiceProvider.GetRequiredService<IBlockService>();
        var settings = ApiTestBase.Scope.ServiceProvider.GetRequiredService<
            IOptions<AppSettings>
        >();
        _settings = settings.Value.Challenge;

        Silo.AddProbe<IChallengeInboxGrain>(id =>
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
    public async Task CreateAsync_rejects_when_the_requester_already_has_a_request_to_the_recipient()
    {
        await ApiTestBase.DbContext.AddRangeAsync(_requester, _recipient);
        await ApiTestBase.DbContext.SaveChangesAsync(ApiTestBase.CT);
        var grain = await CreateGrainAsync();

        await grain.CreateAsync(_requesterId, _recipientId, new PoolKeyFaker().Generate());
        var result = await grain.CreateAsync(
            _requesterId,
            _recipientId,
            new PoolKeyFaker().Generate()
        );

        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(ChallengeErrors.AlreadyExists);
    }

    [Fact]
    public async Task CreateAsync_allows_when_the_recipient_already_has_a_request_from_another_requester()
    {
        var anotherRequester = new AuthedUserFaker().Generate();
        await ApiTestBase.DbContext.AddRangeAsync(_requester, _recipient, anotherRequester);
        await ApiTestBase.DbContext.SaveChangesAsync(ApiTestBase.CT);
        var grain = await CreateGrainAsync();

        await grain.CreateAsync(anotherRequester.Id, _recipientId, new PoolKeyFaker().Generate());
        var result = await grain.CreateAsync(
            _requesterId,
            _recipientId,
            new PoolKeyFaker().Generate()
        );

        result.IsError.Should().BeFalse();
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
        result.Value.Should().Be(expectedChallenge);
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
                TimeSpan.MaxValue
            )
        );
    }

    [Fact]
    public async Task CancelAsync_rejects_when_cancelled_by_is_not_requester_or_recipient()
    {
        var grain = await CreateGrainAsync();
        await CreateAsync(grain);

        var result = await grain.CancelAsync(cancelledBy: "some random");

        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(ChallengeErrors.CannotCancel);
    }

    [Theory]
    [InlineData(_requesterId)]
    [InlineData(_recipientId)]
    public async Task CancelAsync_tears_challenge_down_correctly(string cancelledBy)
    {
        var grain = await CreateGrainAsync();
        await CreateAsync(grain);

        var result = await grain.CancelAsync(cancelledBy);

        result.IsError.Should().BeFalse();

        await _challengeNotifierMock
            .Received(1)
            .NotifyChallengeCancelled(_requesterId, _recipientId, _challengeId);
        await AssertToreDownAsync(grain);
    }

    [Fact]
    public async Task AcceptAsync_rejects_when_done_by_non_recipient()
    {
        var grain = await CreateGrainAsync();
        await CreateAsync(grain);

        var result = await grain.AcceptAsync(_requesterId);

        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(ChallengeErrors.CannotAccept);
    }

    [Fact]
    public async Task AcceptAsync_tears_down_and_creates_game()
    {
        var grain = await CreateGrainAsync();
        var pool = await CreateAsync(grain);

        var result = await grain.AcceptAsync(_recipientId);

        result.IsError.Should().BeFalse();
        var gameToken = result.Value;

        await _challengeNotifierMock
            .Received(1)
            .NotifyChallengeAccepted(_requesterId, gameToken, _challengeId);
        await AssertToreDownAsync(grain);

        var gameStateResult = await _grainFactory.GetGrain<IGameGrain>(gameToken).GetStateAsync();
        gameStateResult.IsError.Should().BeFalse();
        var gameState = gameStateResult.Value;
        gameState.Pool.Should().Be(pool);

        string[] playerUserIds = [gameState.WhitePlayer.UserId, gameState.BlackPlayer.UserId];
        playerUserIds.Should().BeEquivalentTo([_requesterId, _recipientId]);
    }

    [Fact]
    public async Task ReceiveReminder_cancels()
    {
        var grain = await CreateGrainAsync();
        await CreateAsync(grain);

        await Silo.FireAllReminders();

        await _challengeNotifierMock
            .Received(1)
            .NotifyChallengeCancelled(_requesterId, _recipientId, _challengeId);
        await AssertToreDownAsync(grain);
    }

    private async Task<PoolKey> CreateAsync(ChallengeGrain grain)
    {
        await ApiTestBase.DbContext.AddRangeAsync(_requester, _recipient);
        await ApiTestBase.DbContext.SaveChangesAsync(ApiTestBase.CT);

        var pool = new PoolKeyFaker().Generate();
        var result = await grain.CreateAsync(_requesterId, _recipientId, pool);

        result.IsError.Should().BeFalse();
        return pool;
    }

    private async Task AssertToreDownAsync(ChallengeGrain grain)
    {
        var recipientInbox = await _recipientInbox.GetIncomingChallengesAsync();
        recipientInbox.Should().BeEmpty();

        _stateStats.Clears.Should().Be(1);

        Silo.ReminderRegistry.Mock.Verify(x =>
            x.UnregisterReminder(
                Silo.GetGrainId(grain),
                It.Is<IGrainReminder>(r => r.ReminderName == ChallengeGrain.TimeoutReminderName)
            )
        );
    }
}
