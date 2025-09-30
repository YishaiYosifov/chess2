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
    Task<ErrorOr<Created>> CreateAsync(UserId requester, UserId recipient, PoolKey poolKey);

    [Alias("CancelAsync")]
    Task CancelAsync();

    [Alias("AcceptAsync")]
    Task<ErrorOr<Success>> AcceptAsync(UserId acceptedBy);
}

[GenerateSerializer]
[Alias("Chess2.Api.Challenges.Grains.ChallengeState")]
public class ChallengeState
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
    private readonly IPersistentState<ChallengeState> _state;
    private readonly ChallengeSettings _settings;

    private readonly IChallengeNotifier _challengeNotifier;
    private readonly IInteractionLevelGate _interactionLevelGate;
    private readonly UserManager<AuthedUser> _userManager;
    private readonly IGameStarter _gameStarter;
    private readonly TimeProvider _timeProvider;

    public ChallengeGrain(
        ILogger<ChallengeGrain> logger,
        [PersistentState(StateName, StorageNames.ChallengeState)]
            IPersistentState<ChallengeState> state,
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

    public async Task<ErrorOr<Created>> CreateAsync(
        UserId requesterId,
        UserId recipientId,
        PoolKey poolKey
    )
    {
        if (requesterId == recipientId)
            return ChallengeErrors.CannotChallengeSelf;

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
            period: TimeSpan.Zero
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
        await GrainFactory
            .GetGrain<IChallengeInboxGrain>(recipientId)
            .RecordChallengeCreatedAsync(challenge);

        _logger.LogInformation(
            "Challenge {ChallengeId} created by {RequesterId} for {RecipientId}, expires at {ExpiresAt}",
            challenge.ChallengeId,
            requesterId,
            recipientId,
            expiresAt
        );

        return Result.Created;
    }

    public async Task CancelAsync()
    {
        if (_state.State.Request is null)
            return;

        await GrainFactory
            .GetGrain<IChallengeInboxGrain>(_state.State.Request.Recipient.UserId)
            .RecordChallengeRemovedAsync(_challengeId);
        await _challengeNotifier.NotifyChallengeCancelled(
            _state.State.Request.Requester.UserId,
            _state.State.Request.Recipient.UserId,
            _challengeId
        );
        await _state.ClearStateAsync();
        await StopExpirationReminderAsync();
    }

    public async Task<ErrorOr<Success>> AcceptAsync(UserId acceptedBy)
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

        await _state.ClearStateAsync();
        await StopExpirationReminderAsync();
        return Result.Success;
    }

    public async Task ReceiveReminder(string reminderName, TickStatus status)
    {
        if (reminderName != TimeoutReminderName)
            return;
        await CancelAsync();
    }

    private async Task StopExpirationReminderAsync()
    {
        var reminder = await this.GetReminder(TimeoutReminderName);
        if (reminder is not null)
            await this.UnregisterReminder(reminder);
    }
}
