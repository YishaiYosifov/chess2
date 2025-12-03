using AnarchyChess.Api.Challenges.Errors;
using AnarchyChess.Api.Challenges.Grains;
using AnarchyChess.Api.Challenges.Models;
using AnarchyChess.Api.Challenges.Services;
using AnarchyChess.Api.Game.Grains;
using AnarchyChess.Api.Game.Models;
using AnarchyChess.Api.Game.Services;
using AnarchyChess.Api.Matchmaking.Models;
using AnarchyChess.Api.Profile.Entities;
using AnarchyChess.Api.Profile.Models;
using AnarchyChess.Api.Shared.Models;
using AnarchyChess.Api.TestInfrastructure;
using AnarchyChess.Api.TestInfrastructure.Fakes;
using AwesomeAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using NSubstitute;
using Orleans.TestKit;
using Orleans.TestKit.Storage;

namespace AnarchyChess.Api.Integration.Tests.ChallengeTests;

public class ChallengeGrainTests : BaseOrleansIntegrationTest
{
    private const string _requesterId = "requester-id";
    private readonly ChallengeInboxGrain _requesterInbox = new();
    private readonly AuthedUser _requester;

    private const string _recipientId = "recipient-id";
    private readonly ChallengeInboxGrain _recipientInbox = new();
    private readonly AuthedUser _recipient;

    private readonly ChallengeToken _challengeToken = "test challenge";

    private readonly IGrainFactory _grainFactory;
    private readonly ChallengeSettings _settings;

    private readonly IChallengeNotifier _challengeNotifierMock =
        Substitute.For<IChallengeNotifier>();

    private readonly ChallengeGrainStorage _state;
    private readonly TestStorageStats _stateStats;

    public ChallengeGrainTests(AnarchyChessWebApplicationFactory factory)
        : base(factory)
    {
        _requester = new AuthedUserFaker().RuleFor(x => x.Id, (UserId)_requesterId).Generate();
        _recipient = new AuthedUserFaker().RuleFor(x => x.Id, (UserId)_recipientId).Generate();

        _grainFactory = ApiTestBase.Scope.ServiceProvider.GetRequiredService<IGrainFactory>();
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

        Silo.ServiceProvider.AddService(settings);
        Silo.ServiceProvider.AddService(_challengeNotifierMock);
        Silo.ServiceProvider.AddService(
            ApiTestBase.Scope.ServiceProvider.GetRequiredService<IGameStarter>()
        );

        _state = Silo
            .StorageManager.GetStorage<ChallengeGrainStorage>(ChallengeGrain.StateName)
            .State;
        _stateStats = Silo.StorageManager.GetStorageStats(ChallengeGrain.StateName)!;
    }

    private Task<ChallengeGrain> CreateGrainAsync() =>
        Silo.CreateGrainAsync<ChallengeGrain>(_challengeToken);

    [Fact]
    public async Task CreateAsync_sets_challenge_up_correctly()
    {
        var grain = await CreateGrainAsync();

        var challenge = await CreateAsync(grain, _requester, _recipient);

        await _challengeNotifierMock
            .Received(1)
            .NotifyChallengeReceived(recipientId: _recipientId, challenge);

        var requesterInbox = await _requesterInbox.GetIncomingChallengesAsync();
        requesterInbox.Should().BeEmpty();

        var recipientInbox = await _recipientInbox.GetIncomingChallengesAsync();
        recipientInbox.Should().ContainSingle().Which.Should().Be(challenge);

        _state.Request.Should().Be(challenge);
        _stateStats.Writes.Should().Be(1);

        Silo.ReminderRegistry.Mock.Verify(x =>
            x.RegisterOrUpdateReminder(
                Silo.GetGrainId(grain),
                ChallengeGrain.TimeoutReminderName,
                _settings.ChallengeLifetime,
                _settings.ChallengeLifetime
            )
        );
    }

    [Fact]
    public async Task CreateAsync_allows_open_challenge_without_recipient()
    {
        var grain = await CreateGrainAsync();

        var challenge = await CreateAsync(grain, _requester, recipient: null);

        await _challengeNotifierMock
            .DidNotReceiveWithAnyArgs()
            .NotifyChallengeReceived(recipientId: default!, default!);
        var recipientInbox = await _recipientInbox.GetIncomingChallengesAsync();
        recipientInbox.Should().BeEmpty();

        _state.Request.Should().Be(challenge);
        _stateStats.Writes.Should().Be(1);
    }

    [Theory]
    [InlineData(_requesterId)]
    [InlineData(_recipientId)]
    public async Task SubscribeAsync_subscribes_user_to_notification(string userId)
    {
        var grain = await CreateGrainAsync();
        var challenge = await CreateAsync(grain, _requester, _recipient);

        ConnectionId connId = "conn id";
        var result = await grain.SubscribeAsync(userId, connId, ApiTestBase.CT);

        result.IsError.Should().BeFalse();
        await _challengeNotifierMock
            .Received(1)
            .SubscribeToChallengeAsync(connId, challenge.ChallengeToken);
    }

    [Fact]
    public async Task SubscribeAsync_allows_any_user_when_open_challenge()
    {
        var grain = await CreateGrainAsync();
        var challenge = await CreateAsync(grain, _requester, recipient: null);

        ConnectionId connId = "test conn id";
        var result = await grain.SubscribeAsync("random user", connId, ApiTestBase.CT);

        result.IsError.Should().BeFalse();
        await _challengeNotifierMock
            .Received(1)
            .SubscribeToChallengeAsync(connId, challenge.ChallengeToken);
    }

    [Fact]
    public async Task SubscribeAsync_rejects_when_user_is_not_requester_or_recipient()
    {
        var grain = await CreateGrainAsync();
        await CreateAsync(grain, _requester, _recipient);

        var result = await grain.SubscribeAsync("random user", "conn id", ApiTestBase.CT);

        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(ChallengeErrors.NotFound);
        await _challengeNotifierMock
            .DidNotReceiveWithAnyArgs()
            .SubscribeToChallengeAsync(default, default);
    }

    [Theory]
    [InlineData(_requesterId)]
    [InlineData(_recipientId)]
    public async Task GetAsync_returns_challenge(string requestedBy)
    {
        var grain = await CreateGrainAsync();
        var challenge = await CreateAsync(grain, _requester, _recipient);

        var result = await grain.GetAsync(requestedBy: requestedBy, token: ApiTestBase.CT);

        result.IsError.Should().BeFalse();
        result.Value.Should().BeEquivalentTo(challenge);
    }

    [Fact]
    public async Task GetAsync_allows_any_user_to_view_open_challenge()
    {
        var grain = await CreateGrainAsync();
        var challenge = await CreateAsync(grain, _requester, recipient: null);

        var result = await grain.GetAsync("random-user", ApiTestBase.CT);

        result.IsError.Should().BeFalse();
        result.Value.Should().BeEquivalentTo(challenge);
    }

    [Fact]
    public async Task GetAsync_rejects_when_requested_by_is_not_requester_or_recipient()
    {
        var grain = await CreateGrainAsync();
        await CreateAsync(grain, _requester, _recipient);

        var result = await grain.GetAsync(requestedBy: "some random", token: ApiTestBase.CT);

        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(ChallengeErrors.NotFound);
    }

    [Fact]
    public async Task CancelAsync_rejects_when_cancelled_by_is_not_requester_or_recipient()
    {
        var grain = await CreateGrainAsync();
        await CreateAsync(grain, _requester, _recipient);

        var result = await grain.CancelAsync(cancelledBy: "some random", token: ApiTestBase.CT);

        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(ChallengeErrors.NotFound);
    }

    [Theory]
    [InlineData(_requesterId)]
    [InlineData(_recipientId)]
    public async Task CancelAsync_tears_challenge_down_correctly(string cancelledBy)
    {
        var grain = await CreateGrainAsync();
        await CreateAsync(grain, _requester, _recipient);

        var result = await grain.CancelAsync(cancelledBy, ApiTestBase.CT);

        result.IsError.Should().BeFalse();

        await _challengeNotifierMock
            .Received(1)
            .NotifyChallengeCancelled(cancelledBy, _recipientId, _challengeToken);
        await AssertToreDownAsync(grain);
    }

    [Fact]
    public async Task AcceptAsync_rejects_when_done_by_non_recipient()
    {
        var grain = await CreateGrainAsync();
        await CreateAsync(grain, _requester, _recipient);

        var result = await grain.AcceptAsync(_requesterId, ApiTestBase.CT);

        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(ChallengeErrors.NotFound);
    }

    [Fact]
    public async Task AcceptAsync_tears_down_and_creates_game()
    {
        var grain = await CreateGrainAsync();
        var challenge = await CreateAsync(grain, _requester, _recipient);

        var result = await grain.AcceptAsync(_recipientId, ApiTestBase.CT);

        result.IsError.Should().BeFalse();

        await AssertToreDownAsync(grain);
        await AssertGameCreated(
            gameToken: result.Value,
            fromChallenge: challenge,
            requesterId: _requesterId,
            recipientId: _recipientId
        );
    }

    [Fact]
    public async Task AcceptAsync_allows_any_user_to_accept_open_challenge()
    {
        var grain = await CreateGrainAsync();
        var challenge = await CreateAsync(
            grain,
            _requester,
            recipient: null,
            pool: new PoolKeyFaker().RuleFor(x => x.PoolType, PoolType.Casual)
        );

        var result = await grain.AcceptAsync("random-user", ApiTestBase.CT);

        result.IsError.Should().BeFalse();

        await AssertToreDownAsync(grain);
        await AssertGameCreated(
            gameToken: result.Value,
            fromChallenge: challenge,
            requesterId: _requesterId,
            recipientId: "random-user"
        );
    }

    [Fact]
    public async Task AcceptAsync_rejects_guests_for_rated_challenges()
    {
        var grain = await CreateGrainAsync();
        var challenge = await CreateAsync(
            grain,
            _requester,
            recipient: null,
            pool: new PoolKeyFaker().RuleFor(x => x.PoolType, PoolType.Rated)
        );

        var result = await grain.AcceptAsync(UserId.Guest(), ApiTestBase.CT);

        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(ChallengeErrors.AuthedOnlyPool);
    }

    [Fact]
    public async Task ReceiveReminder_cancels()
    {
        var grain = await CreateGrainAsync();
        await CreateAsync(grain, _requester, _recipient);

        await Silo.FireAllReminders();

        await _challengeNotifierMock
            .Received(1)
            .NotifyChallengeCancelled(cancelledBy: null, _recipientId, _challengeToken);
        await AssertToreDownAsync(grain);
    }

    [Fact]
    public async Task ReceiveReminder_cancels_open_challenge()
    {
        var grain = await CreateGrainAsync();
        await CreateAsync(grain, _requester, recipient: null);

        await Silo.FireAllReminders();

        await _challengeNotifierMock
            .Received(1)
            .NotifyChallengeCancelled(cancelledBy: null, recipientId: null, _challengeToken);
        await AssertToreDownAsync(grain);
    }

    private async Task<ChallengeRequest> CreateAsync(
        ChallengeGrain grain,
        AuthedUser requester,
        AuthedUser? recipient,
        PoolKey? pool = null
    )
    {
        await ApiTestBase.DbContext.AddAsync(requester, ApiTestBase.CT);
        if (recipient is not null)
            await ApiTestBase.DbContext.AddAsync(recipient, ApiTestBase.CT);
        await ApiTestBase.DbContext.SaveChangesAsync(ApiTestBase.CT);

        pool ??= new PoolKeyFaker().Generate();
        ChallengeRequest challenge = new(
            _challengeToken,
            Requester: new(requester),
            Recipient: recipient is null ? null : new(recipient),
            Pool: pool,
            ExpiresAt: DateTime.UtcNow + _settings.ChallengeLifetime
        );
        await grain.CreateAsync(challenge);

        return challenge;
    }

    private async Task AssertGameCreated(
        GameToken gameToken,
        ChallengeRequest fromChallenge,
        UserId requesterId,
        UserId recipientId
    )
    {
        await _challengeNotifierMock
            .Received(1)
            .NotifyChallengeAccepted(gameToken, _challengeToken);

        var gameStateResult = await _grainFactory.GetGrain<IGameGrain>(gameToken).GetStateAsync();
        gameStateResult.IsError.Should().BeFalse();
        var gameState = gameStateResult.Value;
        gameState.Pool.Should().Be(fromChallenge.Pool);
        gameState.GameSource.Should().Be(GameSource.Challenge);

        UserId[] playerUserIds = [gameState.WhitePlayer.UserId, gameState.BlackPlayer.UserId];
        playerUserIds.Should().BeEquivalentTo([requesterId, recipientId]);
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
