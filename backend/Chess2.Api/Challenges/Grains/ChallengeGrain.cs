using Chess2.Api.Challenges.Errors;
using Chess2.Api.Challenges.Models;
using Chess2.Api.GameSnapshot.Models;
using Chess2.Api.Infrastructure;
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
    Task<ErrorOr<Created>> CreateAsync(
        UserId requester,
        UserId recipient,
        TimeControlSettings timeControl
    );

    [Alias("CancelAsync")]
    Task CancelAsync();
}

[GenerateSerializer]
[Alias("Chess2.Api.Challenges.Grains.ChallengeState")]
public class ChallengeState
{
    [Id(0)]
    public UserId? RecipientId { get; set; }

    [Id(1)]
    public TimeControlSettings TimeControl { get; set; }
}

public class ChallengeGrain(
    ILogger<ChallengeGrain> logger,
    [PersistentState(ChallengeGrain.StateName, StorageNames.ChallengeState)]
        IPersistentState<ChallengeState> state,
    IOptions<AppSettings> settings,
    TimeProvider timeProvider,
    UserManager<AuthedUser> userManager,
    IInteractionLevelGate interactionLevelGate
) : Grain, IChallengeGrain, IRemindable
{
    public const string TimeoutReminderName = "ChallengeTimeoutReminder";
    public const string StateName = "challenge";

    private readonly ILogger<ChallengeGrain> _logger = logger;
    private readonly IPersistentState<ChallengeState> _state = state;
    private readonly TimeProvider _timeProvider = timeProvider;
    private readonly UserManager<AuthedUser> _userManager = userManager;
    private readonly IInteractionLevelGate _interactionLevelGate = interactionLevelGate;
    private readonly ChallengeSettings _settings = settings.Value.Challenge;

    public async Task<ErrorOr<Created>> CreateAsync(
        UserId requesterId,
        UserId recipientId,
        TimeControlSettings timeControl
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

        _state.State.RecipientId = recipientId;
        _state.State.TimeControl = timeControl;
        await _state.WriteStateAsync();

        IncomingChallenge challenge = new(
            ChallengeId: this.GetPrimaryKeyString(),
            Requester: new MinimalProfile(requester),
            TimeControl: timeControl,
            ExpiresAt: expiresAt
        );
        await GrainFactory
            .GetGrain<IChallengeInboxGrain>(recipientId)
            .ChallengeCreatedAsync(challenge);

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
        if (_state.State.RecipientId is null)
            return;

        await GrainFactory
            .GetGrain<IChallengeInboxGrain>(_state.State.RecipientId)
            .ChallengeCanceledAsync(this.GetPrimaryKeyString());
        await _state.ClearStateAsync();

        var reminder = await this.GetReminder(TimeoutReminderName);
        if (reminder is not null)
            await this.UnregisterReminder(reminder);
    }

    public async Task ReceiveReminder(string reminderName, TickStatus status)
    {
        if (reminderName != TimeoutReminderName)
            return;
        await CancelAsync();
    }
}
