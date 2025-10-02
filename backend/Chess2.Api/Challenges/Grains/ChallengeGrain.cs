using Chess2.Api.Challenges.Errors;
using Chess2.Api.Challenges.Models;
using Chess2.Api.Challenges.Services;
using Chess2.Api.Infrastructure;
using Chess2.Api.LiveGame.Services;
using Chess2.Api.Matchmaking.Models;
using Chess2.Api.Preferences.Services;
using Chess2.Api.Profile.DTOs;
using Chess2.Api.Profile.Entities;
using Chess2.Api.Profile.Errors;
using Chess2.Api.Profile.Models;
using Chess2.Api.Shared.Models;
using ErrorOr;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Chess2.Api.Challenges.Grains;

[Alias("Chess2.Api.Challenges.Grains.IChallengeGrain")]
public interface IChallengeGrain : IGrainWithStringKey
{
    [Alias("CreateAsync")]
    Task<ErrorOr<ChallengeRequest>> CreateAsync(
        UserId requester,
        UserId recipient,
        PoolKey poolKey
    );

    [Alias("GetAsync")]
    public Task<ErrorOr<ChallengeRequest>> GetAsync(UserId requestedBy);

    [Alias("CancelAsync")]
    Task<ErrorOr<Deleted>> CancelAsync(UserId cancelledBy);

    [Alias("AcceptAsync")]
    Task<ErrorOr<string>> AcceptAsync(UserId acceptedBy);
}

[GenerateSerializer]
[Alias("Chess2.Api.Challenges.Grains.ChallengeGrainStorage")]
public class ChallengeGrainStorage
{
    [Id(0)]
    public ChallengeRequest? Request { get; set; }
}

public class ChallengeGrain : Grain, IChallengeGrain, IRemindable
{
    public const string TimeoutReminderName = "ChallengeTimeoutReminder";
    public const string StateName = "challenge";

    private readonly ChallengeId _challengeId;

    private readonly ILogger<ChallengeGrain> _logger;
    private readonly IPersistentState<ChallengeGrainStorage> _state;
    private readonly ChallengeSettings _settings;

    private readonly IChallengeNotifier _challengeNotifier;
    private readonly IInteractionLevelGate _interactionLevelGate;
    private readonly UserManager<AuthedUser> _userManager;
    private readonly IGameStarter _gameStarter;
    private readonly TimeProvider _timeProvider;

    public ChallengeGrain(
        ILogger<ChallengeGrain> logger,
        [PersistentState(StateName, StorageNames.ChallengeState)]
            IPersistentState<ChallengeGrainStorage> state,
        IOptions<AppSettings> settings,
        IChallengeNotifier challengeNotifier,
        IInteractionLevelGate interactionLevelGate,
        UserManager<AuthedUser> userManager,
        IGameStarter gameStarter,
        TimeProvider timeProvider
    )
    {
        _logger = logger;
        _state = state;
        _settings = settings.Value.Challenge;
        _challengeNotifier = challengeNotifier;
        _interactionLevelGate = interactionLevelGate;
        _userManager = userManager;
        _gameStarter = gameStarter;
        _timeProvider = timeProvider;

        _challengeId = this.GetPrimaryKeyString();
    }

    public async Task<ErrorOr<ChallengeRequest>> CreateAsync(
        UserId requesterId,
        UserId recipientId,
        PoolKey poolKey
    )
    {
        if (requesterId == recipientId)
            return ChallengeErrors.CannotChallengeSelf;

        var recipientInbox = GrainFactory.GetGrain<IChallengeInboxGrain>(recipientId);
        if (await IsDuplicateChallenge(recipientInbox, requesterId))
            return ChallengeErrors.AlreadyExists;

        var recipient = await _userManager.FindByIdAsync(recipientId);
        if (recipient is null)
            return ProfileErrors.NotFound;

        var requester = await _userManager.FindByIdAsync(requesterId);
        if (requester is null)
            return Error.Unauthorized();

        var canInteractWith = await _interactionLevelGate.CanInteractWithAsync(
            prefs => prefs.ChallengePreference,
            requesterId: requesterId,
            recipientId: recipientId
        );
        if (!canInteractWith)
            return ChallengeErrors.RecipientNotAccepting;

        var expiresAt = _timeProvider.GetUtcNow().DateTime + _settings.ChallengeLifetime;
        await this.RegisterOrUpdateReminder(
            TimeoutReminderName,
            dueTime: _settings.ChallengeLifetime,
            period: TimeSpan.MaxValue
        );

        ChallengeRequest challenge = new(
            ChallengeId: _challengeId,
            Requester: new MinimalProfile(requester),
            Recipient: new MinimalProfile(recipient),
            Pool: poolKey,
            ExpiresAt: expiresAt
        );
        _state.State.Request = challenge;
        await _state.WriteStateAsync();

        await _challengeNotifier.NotifyChallengeReceived(recipientId: recipientId, challenge);
        await recipientInbox.RecordChallengeCreatedAsync(challenge);

        _logger.LogInformation(
            "Challenge {ChallengeId} created by {RequesterId} for {RecipientId}, expires at {ExpiresAt}",
            challenge.ChallengeId,
            requesterId,
            recipientId,
            expiresAt
        );

        return challenge;
    }

    public Task<ErrorOr<ChallengeRequest>> GetAsync(UserId requestedBy)
    {
        if (
            requestedBy.Value != _state.State.Request?.Recipient.UserId
            && requestedBy.Value != _state.State.Request?.Requester.UserId
        )
            return Task.FromResult<ErrorOr<ChallengeRequest>>(ChallengeErrors.NotFound);

        return Task.FromResult<ErrorOr<ChallengeRequest>>(_state.State.Request);
    }

    public async Task<ErrorOr<Deleted>> CancelAsync(UserId cancelledBy)
    {
        if (
            cancelledBy.Value != _state.State.Request?.Recipient.UserId
            && cancelledBy.Value != _state.State.Request?.Requester.UserId
        )
            return ChallengeErrors.NotFound;

        _logger.LogInformation(
            "Challenge {ChallengeId} cancelled by {CancelledBy}",
            _challengeId,
            cancelledBy
        );

        await ApplyCancellationAsync();
        return Result.Deleted;
    }

    public async Task<ErrorOr<string>> AcceptAsync(UserId acceptedBy)
    {
        if (acceptedBy.Value != _state.State.Request?.Recipient.UserId)
            return ChallengeErrors.CannotAccept;

        var gameToken = await _gameStarter.StartGameAsync(
            _state.State.Request.Requester.UserId,
            _state.State.Request.Recipient.UserId,
            _state.State.Request.Pool
        );
        await _challengeNotifier.NotifyChallengeAccepted(
            _state.State.Request.Requester.UserId,
            gameToken,
            _challengeId
        );

        await TearDownChallengeAsync();
        _logger.LogInformation("Challenge {ChallengeId} accepted", _challengeId);

        return gameToken;
    }

    public async Task ReceiveReminder(string reminderName, TickStatus status)
    {
        if (reminderName != TimeoutReminderName)
            return;

        _logger.LogInformation("Challenge {ChallengeId} expired", _challengeId);
        await ApplyCancellationAsync();
    }

    private static async Task<bool> IsDuplicateChallenge(
        IChallengeInboxGrain recipientInbox,
        UserId requesterId
    )
    {
        var recipientChallenges = await recipientInbox.GetIncomingChallengesAsync();
        return recipientChallenges.Any(x => x.Requester.UserId == requesterId);
    }

    private async Task ApplyCancellationAsync()
    {
        if (_state.State.Request is null)
            return;

        await _challengeNotifier.NotifyChallengeCancelled(
            _state.State.Request.Requester.UserId,
            _state.State.Request.Recipient.UserId,
            _challengeId
        );
        await TearDownChallengeAsync();
    }

    private async Task TearDownChallengeAsync()
    {
        if (_state.State.Request is null)
            return;

        await GrainFactory
            .GetGrain<IChallengeInboxGrain>(_state.State.Request.Recipient.UserId)
            .RecordChallengeRemovedAsync(_challengeId);

        await _state.ClearStateAsync();
        var reminder = await this.GetReminder(TimeoutReminderName);
        if (reminder is not null)
            await this.UnregisterReminder(reminder);
    }
}
