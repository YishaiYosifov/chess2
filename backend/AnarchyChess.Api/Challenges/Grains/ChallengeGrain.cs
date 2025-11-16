using AnarchyChess.Api.Challenges.Errors;
using AnarchyChess.Api.Challenges.Models;
using AnarchyChess.Api.Challenges.Services;
using AnarchyChess.Api.Game.Models;
using AnarchyChess.Api.Game.Services;
using AnarchyChess.Api.Matchmaking.Models;
using AnarchyChess.Api.Profile.Models;
using AnarchyChess.Api.Shared.Models;
using ErrorOr;
using Microsoft.Extensions.Options;

namespace AnarchyChess.Api.Challenges.Grains;

[Alias("AnarchyChess.Api.Challenges.Grains.IChallengeGrain")]
public interface IChallengeGrain : IGrainWithStringKey
{
    [Alias("CreateAsync")]
    Task CreateAsync(ChallengeRequest challenge, CancellationToken token = default);

    [Alias("GetAsync")]
    public Task<ErrorOr<ChallengeRequest>> GetAsync(
        UserId requestedBy,
        CancellationToken token = default
    );

    [Alias("CancelAsync")]
    Task<ErrorOr<Deleted>> CancelAsync(UserId cancelledBy, CancellationToken token = default);

    [Alias("AcceptAsync")]
    Task<ErrorOr<GameToken>> AcceptAsync(UserId acceptedBy, CancellationToken token = default);

    [Alias("SubscribeAsync")]
    Task<ErrorOr<Success>> SubscribeAsync(
        UserId userId,
        ConnectionId connectionId,
        CancellationToken token = default
    );
}

[GenerateSerializer]
[Alias("AnarchyChess.Api.Challenges.Grains.ChallengeGrainStorage")]
public class ChallengeGrainStorage
{
    [Id(0)]
    public ChallengeRequest? Request { get; set; }
}

public class ChallengeGrain : Grain, IChallengeGrain, IRemindable
{
    public const string TimeoutReminderName = "ChallengeTimeoutReminder";
    public const string StateName = "challenge";

    private readonly ChallengeToken _challengeToken;

    private readonly ILogger<ChallengeGrain> _logger;
    private readonly IPersistentState<ChallengeGrainStorage> _state;
    private readonly ChallengeSettings _settings;

    private readonly IChallengeNotifier _challengeNotifier;
    private readonly IGameStarter _gameStarter;

    public ChallengeGrain(
        ILogger<ChallengeGrain> logger,
        [PersistentState(StateName)] IPersistentState<ChallengeGrainStorage> state,
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

        _challengeToken = this.GetPrimaryKeyString();
    }

    public async Task CreateAsync(ChallengeRequest challenge, CancellationToken token = default)
    {
        await this.RegisterOrUpdateReminder(
            TimeoutReminderName,
            dueTime: _settings.ChallengeLifetime,
            period: _settings.ChallengeLifetime
        );

        _state.State.Request = challenge;
        await _state.WriteStateAsync(token);

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
            "Challenge {ChallengeToken} created by {RequesterId} for {RecipientId}, expires at {ExpiresAt}",
            challenge.ChallengeToken,
            challenge.Requester.UserId,
            challenge.Recipient?.UserId,
            challenge.ExpiresAt
        );
    }

    public async Task<ErrorOr<Success>> SubscribeAsync(
        UserId userId,
        ConnectionId connectionId,
        CancellationToken token = default
    )
    {
        var request = _state.State.Request;
        if (request is null)
            return ChallengeErrors.NotFound;

        if (request.Recipient is not null && IsUserSpectator(userId))
            return ChallengeErrors.NotFound;

        await _challengeNotifier.SubscribeToChallengeAsync(connectionId, _challengeToken);
        return Result.Success;
    }

    public Task<ErrorOr<ChallengeRequest>> GetAsync(
        UserId requestedBy,
        CancellationToken token = default
    )
    {
        var request = _state.State.Request;
        if (request is null)
            return Task.FromResult<ErrorOr<ChallengeRequest>>(ChallengeErrors.NotFound);

        if (request.Recipient is not null && IsUserSpectator(requestedBy))
            return Task.FromResult<ErrorOr<ChallengeRequest>>(ChallengeErrors.NotFound);

        return Task.FromResult<ErrorOr<ChallengeRequest>>(request);
    }

    public async Task<ErrorOr<Deleted>> CancelAsync(
        UserId cancelledBy,
        CancellationToken token = default
    )
    {
        if (!IsUserRequester(cancelledBy) && !IsUserRecipient(cancelledBy))
            return ChallengeErrors.NotFound;

        _logger.LogInformation(
            "Challenge {ChallengeToken} cancelled by {CancelledBy}",
            _challengeToken,
            cancelledBy
        );

        await ApplyCancellationAsync(cancelledBy, token);
        return Result.Deleted;
    }

    public async Task<ErrorOr<GameToken>> AcceptAsync(
        UserId acceptedBy,
        CancellationToken token = default
    )
    {
        var request = _state.State.Request;
        if (request is null)
            return ChallengeErrors.NotFound;

        if (request.Recipient is not null && !IsUserRecipient(acceptedBy))
            return ChallengeErrors.NotFound;

        if (request.Pool.PoolType is PoolType.Rated && acceptedBy.IsGuest)
            return ChallengeErrors.AuthedOnlyPool;

        var gameToken = await _gameStarter.StartGameAsync(
            userId1: request.Requester.UserId,
            userId2: acceptedBy,
            request.Pool,
            GameSource.Challenge,
            token: token
        );
        await _challengeNotifier.NotifyChallengeAccepted(gameToken, _challengeToken);

        await TearDownChallengeAsync(token);
        _logger.LogInformation("Challenge {ChallengeToken} accepted", _challengeToken);

        return gameToken;
    }

    public async Task ReceiveReminder(string reminderName, TickStatus status)
    {
        if (reminderName != TimeoutReminderName)
            return;

        _logger.LogInformation("Challenge {ChallengeToken} expired", _challengeToken);
        await ApplyCancellationAsync(cancelledBy: null);
    }

    private async Task ApplyCancellationAsync(
        UserId? cancelledBy,
        CancellationToken token = default
    )
    {
        if (_state.State.Request is not null)
            await _challengeNotifier.NotifyChallengeCancelled(
                cancelledBy: cancelledBy,
                _challengeToken
            );

        await TearDownChallengeAsync(token);
    }

    private async Task TearDownChallengeAsync(CancellationToken token = default)
    {
        if (_state.State.Request?.Recipient is not null)
        {
            await GrainFactory
                .GetGrain<IChallengeInboxGrain>(_state.State.Request.Recipient.UserId)
                .RecordChallengeRemovedAsync(_challengeToken);
        }

        await _state.ClearStateAsync(token);
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
