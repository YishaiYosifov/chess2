using Chess2.Api.Preferences.DTOs;
using Chess2.Api.Preferences.Models;
using Chess2.Api.Profile.Models;
using Chess2.Api.Social.Services;

namespace Chess2.Api.Preferences.Services;

public interface IInteractionLevelGate
{
    Task<bool> CanInteractWithAsync(
        Func<PreferenceDto, InteractionLevel> getInteractionLevel,
        UserId requesterId,
        UserId recipientId
    );
}

public class InteractionLevelGate(
    IPreferenceService preferenceService,
    IBlockService blockService,
    IStarService starService
) : IInteractionLevelGate
{
    private readonly IPreferenceService _preferenceService = preferenceService;
    private readonly IBlockService _blockService = blockService;
    private readonly IStarService _starService = starService;

    public async Task<bool> CanInteractWithAsync(
        Func<PreferenceDto, InteractionLevel> getInteractionLevel,
        UserId requesterId,
        UserId recipientId
    )
    {
        var preference = await _preferenceService.GetPreferencesAsync(recipientId);
        var interactionLevel = getInteractionLevel(preference);
        if (interactionLevel is InteractionLevel.NoOne)
            return false;

        if (await _blockService.HasBlockedAsync(byUserId: recipientId, blockedUserId: requesterId))
            return false;

        if (
            interactionLevel is InteractionLevel.Starred
            && !await _starService.HasStarredAsync(
                byUserId: recipientId,
                starredUserId: requesterId
            )
        )
            return false;

        return true;
    }
}
