using Chess2.Api.Auth.Services;
using Chess2.Api.Infrastructure.Extensions;
using Chess2.Api.Preferences.DTOs;
using Chess2.Api.Preferences.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Chess2.Api.Preferences.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PreferenceController(IPreferenceService preferenceService, IAuthService authService)
    : ControllerBase
{
    private readonly IPreferenceService _preferenceService = preferenceService;
    private readonly IAuthService _authService = authService;

    [HttpGet(Name = nameof(GetPreferences))]
    [ProducesResponseType<PreferenceDto>(StatusCodes.Status200OK)]
    [Authorize]
    public async Task<ActionResult<PreferenceDto>> GetPreferences(CancellationToken token)
    {
        var userIdResult = _authService.GetUserId(User);
        if (userIdResult.IsError)
            return userIdResult.Errors.ToActionResult();

        var preferences = await _preferenceService.GetPreferencesAsync(userIdResult.Value, token);
        return Ok(preferences);
    }

    [HttpPut(Name = nameof(SetPreferences))]
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
