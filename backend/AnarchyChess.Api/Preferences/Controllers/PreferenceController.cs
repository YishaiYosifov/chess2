using AnarchyChess.Api.Auth.Services;
using AnarchyChess.Api.ErrorHandling.Extensions;
using AnarchyChess.Api.Infrastructure;
using AnarchyChess.Api.Preferences.DTOs;
using AnarchyChess.Api.Preferences.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AnarchyChess.Api.Preferences.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PreferenceController(
    IPreferenceService preferenceService,
    IAuthService authService,
    IGuestService guestService
) : Controller
{
    private readonly IPreferenceService _preferenceService = preferenceService;
    private readonly IAuthService _authService = authService;
    private readonly IGuestService _guestService = guestService;

    [HttpGet]
    [ProducesResponseType<PreferenceDto>(StatusCodes.Status200OK)]
    [Authorize(AuthPolicies.ActiveSession)]
    public async Task<ActionResult<PreferenceDto>> GetPreferences(CancellationToken token)
    {
        if (_guestService.IsGuest(User))
            return Ok(PreferenceDto.Default);

        var userIdResult = _authService.GetUserId(User);
        if (userIdResult.IsError)
            return userIdResult.Errors.ToActionResult();

        var preferences = await _preferenceService.GetPreferencesAsync(userIdResult.Value, token);
        return Ok(preferences);
    }

    [HttpPut]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [Authorize]
    public async Task<ActionResult> SetPreferences(
        PreferenceDto preferences,
        CancellationToken token
    )
    {
        var userIdResult = _authService.GetUserId(User);
        if (userIdResult.IsError)
            return userIdResult.Errors.ToActionResult();

        await _preferenceService.UpdatePreferencesAsync(userIdResult.Value, preferences, token);
        return NoContent();
    }
}
