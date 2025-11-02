using Chess2.Api.Challenges.Errors;
using Chess2.Api.Challenges.Grains;
using Chess2.Api.Challenges.Models;
using Chess2.Api.GameSnapshot.Services;
using Chess2.Api.Matchmaking.Models;
using Chess2.Api.Preferences.Services;
using Chess2.Api.Profile.DTOs;
using Chess2.Api.Profile.Entities;
using Chess2.Api.Profile.Errors;
using Chess2.Api.Profile.Models;
using Chess2.Api.Shared.Models;
using Chess2.Api.Shared.Services;
using ErrorOr;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Chess2.Api.Challenges.Services;

public interface IChallengeRequestCreator
{
    Task<ErrorOr<ChallengeRequest>> CreateAsync(
        UserId requesterId,
        UserId? recipientId,
        PoolKey pool
    );
}

public class ChallengeRequestCreator(
    IGrainFactory grains,
    IOptions<AppSettings> settings,
    IRandomCodeGenerator randomCodeGenerator,
    ITimeControlTranslator timeControlTranslator,
    IInteractionLevelGate interactionLevelGate,
    TimeProvider timeProvider,
    UserManager<AuthedUser> userManager
) : IChallengeRequestCreator
{
    private readonly ChallengeSettings _settings = settings.Value.Challenge;
    private readonly IGrainFactory _grains = grains;
    private readonly IRandomCodeGenerator _randomCodeGenerator = randomCodeGenerator;
    private readonly ITimeControlTranslator _timeControlTranslator = timeControlTranslator;
    private readonly IInteractionLevelGate _interactionLevelGate = interactionLevelGate;
    private readonly TimeProvider _timeProvider = timeProvider;
    private readonly UserManager<AuthedUser> _userManager = userManager;

    public async Task<ErrorOr<ChallengeRequest>> CreateAsync(
        UserId requesterId,
        UserId? recipientId,
        PoolKey pool
    )
    {
        if (requesterId == recipientId)
            return ChallengeErrors.CannotChallengeSelf;

        if (requesterId.IsGuest && pool.PoolType is PoolType.Rated)
            return ChallengeErrors.AuthedOnlyPool;

        var expiresAt = _timeProvider.GetUtcNow().UtcDateTime + _settings.ChallengeLifetime;
        var requester = await _userManager.FindByIdAsync(requesterId);
        MinimalProfile requesterProfile = new(requesterId, requester);
        ChallengeId challengeId = _randomCodeGenerator.Generate(16);

        var challengeResult = recipientId is null
            ? CreateChallengeWithoutRecipient(challengeId, requesterProfile, pool, expiresAt)
            : await CreateChallengeWithRecipientAsync(
                challengeId,
                requesterProfile,
                recipientId.Value,
                pool,
                expiresAt
            );
        return challengeResult;
    }

    private ChallengeRequest CreateChallengeWithoutRecipient(
        ChallengeId challengeId,
        MinimalProfile requester,
        PoolKey pool,
        DateTime expiresAt
    ) => BuildChallenge(challengeId, requester, recipient: null, pool, expiresAt);

    private async Task<ErrorOr<ChallengeRequest>> CreateChallengeWithRecipientAsync(
        ChallengeId challengeId,
        MinimalProfile requester,
        UserId recipientId,
        PoolKey pool,
        DateTime expiresAt
    )
    {
        var recipient = await _userManager.FindByIdAsync(recipientId);
        if (recipient is null)
            return ProfileErrors.NotFound;

        if (await IsDuplicateChallenge(requesterId: requester.UserId, recipientId: recipientId))
            return ChallengeErrors.AlreadyExists;

        var canInteractWith = await _interactionLevelGate.CanInteractWithAsync(
            prefs => prefs.ChallengePreference,
            requesterId: requester.UserId,
            recipientId: recipientId
        );
        if (!canInteractWith)
            return ChallengeErrors.RecipientNotAccepting;

        ChallengeRequest challenge = BuildChallenge(
            challengeId,
            requester,
            new MinimalProfile(recipient),
            pool,
            expiresAt
        );

        return challenge;
    }

    private ChallengeRequest BuildChallenge(
        ChallengeId challengeId,
        MinimalProfile requester,
        MinimalProfile? recipient,
        PoolKey pool,
        DateTime expiresAt
    ) =>
        new(
            ChallengeId: challengeId,
            Requester: requester,
            Recipient: recipient,
            TimeControl: _timeControlTranslator.FromSeconds(pool.TimeControl.BaseSeconds),
            Pool: pool,
            ExpiresAt: expiresAt
        );

    private async Task<bool> IsDuplicateChallenge(UserId requesterId, UserId recipientId)
    {
        var recipientInbox = _grains.GetGrain<IChallengeInboxGrain>(recipientId);
        var recipientChallenges = await recipientInbox.GetIncomingChallengesAsync();
        return recipientChallenges.Any(x => x.Requester.UserId == requesterId);
    }
}
