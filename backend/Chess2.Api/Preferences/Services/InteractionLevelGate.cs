using Chess2.Api.Preferences.DTOs;
using Chess2.Api.Preferences.Models;
using Chess2.Api.Profile.Models;
using Chess2.Api.Social.Services;

namespace Chess2.Api.Preferences.Services;

public interface IInteractionLevelGate
{
    Task<bool> CanInteractWithAsync(
        Func<PreferenceDto, InteractionLevel> getInteractionLevel,
        UserId requester,
        UserId recipient
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
        UserId requester,
        UserId recipient
    )
    {
        var preference = await _preferenceService.GetPreferencesAsync(recipient);
        var interactionLevel = getInteractionLevel(preference);
        if (interactionLevel is InteractionLevel.NoOne)
            return false;

        if (await _blockService.HasBlockedAsync(byUserId: recipient, blockedUserId: requester))
            return false;

        if (
            interactionLevel is InteractionLevel.Starred
            && !await _starService.HasStarredAsync(byUserId: recipient, starredUserId: requester)
        )
            return false;

        return true;
    }
}
