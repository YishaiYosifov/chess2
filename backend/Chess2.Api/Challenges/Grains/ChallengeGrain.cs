using Chess2.Api.Challenges.Errors;
using Chess2.Api.Challenges.Models;
using Chess2.Api.Challenges.Services;
using Chess2.Api.Infrastructure;
using Chess2.Api.LiveGame.Services;
using Chess2.Api.Matchmaking.Models;
using Chess2.Api.Profile.Models;
using Chess2.Api.Shared.Models;
using ErrorOr;
using Microsoft.Extensions.Options;

namespace Chess2.Api.Challenges.Grains;

[Alias("Chess2.Api.Challenges.Grains.IChallengeGrain")]
public interface IChallengeGrain : IGrainWithStringKey
{
    [Alias("CreateAsync")]
    Task CreateAsync(ChallengeRequest challenge);

    [Alias("GetAsync")]
    public Task<ErrorOr<ChallengeRequest>> GetAsync(UserId requestedBy);

    [Alias("CancelAsync")]
    Task<ErrorOr<Deleted>> CancelAsync(UserId cancelledBy);

    [Alias("AcceptAsync")]
    Task<ErrorOr<string>> AcceptAsync(UserId acceptedBy, bool isGuest);
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
    private readonly IGameStarter _gameStarter;

    public ChallengeGrain(
        ILogger<ChallengeGrain> logger,
        [PersistentState(StateName, StorageNames.ChallengeState)]
            IPersistentState<ChallengeGrainStorage> state,
        IOptions<AppSettings> settings,
        IChallengeNotifier challengeNotifier,
        IGameStarter gameStarter
    )
    {
        _logger = logger;
        _state = state;
        _settings = settings.Value.Challenge;
        _challengeNotifier = challengeNotifier;
        _gameStarter = gameStarter;

        _challengeId = this.GetPrimaryKeyString();
    }

    public async Task CreateAsync(ChallengeRequest challenge)
    {
        await this.RegisterOrUpdateReminder(
            TimeoutReminderName,
            dueTime: _settings.ChallengeLifetime,
            period: _settings.ChallengeLifetime
        );

        _state.State.Request = challenge;
        await _state.WriteStateAsync();

        if (challenge.Recipient is not null)
        {
            await _challengeNotifier.NotifyChallengeReceived(
                recipientId: challenge.Recipient.UserId,
                challenge
            );
            await GrainFactory
                .GetGrain<IChallengeInboxGrain>(challenge.Recipient.UserId)
                .RecordChallengeCreatedAsync(challenge);
        }

        _logger.LogInformation(
            "Challenge {ChallengeId} created by {RequesterId} for {RecipientId}, expires at {ExpiresAt}",
            challenge.ChallengeId,
            challenge.Requester.UserId,
            challenge.Recipient?.UserId,
            challenge.ExpiresAt
        );
    }

    public Task<ErrorOr<ChallengeRequest>> GetAsync(UserId requestedBy)
    {
        var request = _state.State.Request;
        if (request is null)
            return Task.FromResult<ErrorOr<ChallengeRequest>>(ChallengeErrors.NotFound);

        if (request.Recipient is not null && IsUserSpectator(requestedBy))
            return Task.FromResult<ErrorOr<ChallengeRequest>>(ChallengeErrors.NotFound);

        return Task.FromResult<ErrorOr<ChallengeRequest>>(request);
    }

    public async Task<ErrorOr<Deleted>> CancelAsync(UserId cancelledBy)
    {
        if (!IsUserRequester(cancelledBy) && !IsUserRecipient(cancelledBy))
            return ChallengeErrors.NotFound;

        _logger.LogInformation(
            "Challenge {ChallengeId} cancelled by {CancelledBy}",
            _challengeId,
            cancelledBy
        );

        await ApplyCancellationAsync(cancelledBy);
        return Result.Deleted;
    }

    public async Task<ErrorOr<string>> AcceptAsync(UserId acceptedBy, bool isGuest)
    {
        var request = _state.State.Request;
        if (request is null)
            return ChallengeErrors.NotFound;

        if (request.Recipient is not null && !IsUserRecipient(acceptedBy))
            return ChallengeErrors.NotFound;

        if (request.Pool.PoolType is PoolType.Rated && isGuest)
            return ChallengeErrors.AuthedOnlyPool;

        var gameToken = await _gameStarter.StartGameAsync(
            userId1: request.Requester.UserId,
            userId2: acceptedBy,
            request.Pool
        );
        await _challengeNotifier.NotifyChallengeAccepted(
            request.Requester.UserId,
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
        await ApplyCancellationAsync(cancelledBy: null);
    }

    private async Task ApplyCancellationAsync(UserId? cancelledBy)
    {
        if (_state.State.Request is null)
            return;

        await _challengeNotifier.NotifyChallengeCancelled(
            cancelledBy: cancelledBy,
            _state.State.Request.Requester.UserId,
            _state.State.Request.Recipient?.UserId,
            _challengeId
        );
        await TearDownChallengeAsync();
    }

    private async Task TearDownChallengeAsync()
    {
        if (_state.State.Request?.Recipient is not null)
        {
            await GrainFactory
                .GetGrain<IChallengeInboxGrain>(_state.State.Request.Recipient.UserId)
                .RecordChallengeRemovedAsync(_challengeId);
        }

        await _state.ClearStateAsync();
        var reminder = await this.GetReminder(TimeoutReminderName);
        if (reminder is not null)
            await this.UnregisterReminder(reminder);
    }

    private bool IsUserRequester(UserId userId) => userId == _state.State.Request?.Requester.UserId;

    private bool IsUserRecipient(UserId userId) =>
        userId == _state.State.Request?.Recipient?.UserId;

    private bool IsUserSpectator(UserId userId) =>
        userId != _state.State.Request?.Recipient?.UserId
        && userId != _state.State.Request?.Requester.UserId;
}
