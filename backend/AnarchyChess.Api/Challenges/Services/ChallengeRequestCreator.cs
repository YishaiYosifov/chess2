using AnarchyChess.Api.Challenges.Errors;
using AnarchyChess.Api.Challenges.Grains;
using AnarchyChess.Api.Challenges.Models;
using AnarchyChess.Api.GameSnapshot.Services;
using AnarchyChess.Api.Matchmaking.Models;
using AnarchyChess.Api.Preferences.Services;
using AnarchyChess.Api.Profile.DTOs;
using AnarchyChess.Api.Profile.Entities;
using AnarchyChess.Api.Profile.Errors;
using AnarchyChess.Api.Profile.Models;
using AnarchyChess.Api.Shared.Models;
using AnarchyChess.Api.Shared.Services;
using ErrorOr;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace AnarchyChess.Api.Challenges.Services;

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
        ChallengeToken challengeToken = _randomCodeGenerator.Generate(16);

        var challengeResult = recipientId is null
            ? CreateChallengeWithoutRecipient(challengeToken, requesterProfile, pool, expiresAt)
            : await CreateChallengeWithRecipientAsync(
                challengeToken,
                requesterProfile,
                recipientId.Value,
                pool,
                expiresAt
            );
        return challengeResult;
    }

    private ChallengeRequest CreateChallengeWithoutRecipient(
        ChallengeToken challengeToken,
        MinimalProfile requester,
        PoolKey pool,
        DateTime expiresAt
    ) => BuildChallenge(challengeToken, requester, recipient: null, pool, expiresAt);

    private async Task<ErrorOr<ChallengeRequest>> CreateChallengeWithRecipientAsync(
        ChallengeToken challengeToken,
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
            challengeToken,
            requester,
            new MinimalProfile(recipient),
            pool,
            expiresAt
        );

        return challenge;
    }

    private ChallengeRequest BuildChallenge(
        ChallengeToken challengeToken,
        MinimalProfile requester,
        MinimalProfile? recipient,
        PoolKey pool,
        DateTime expiresAt
    ) =>
        new(
            ChallengeToken: challengeToken,
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
