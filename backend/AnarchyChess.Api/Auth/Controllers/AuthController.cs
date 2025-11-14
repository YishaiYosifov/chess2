using AnarchyChess.Api.Auth.Services;
using AnarchyChess.Api.Infrastructure;
using AnarchyChess.Api.Infrastructure.Errors;
using AnarchyChess.Api.Infrastructure.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AnarchyChess.Api.Auth.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(
    IAuthService authService,
    IGuestService guestService,
    IAuthCookieSetter authCookieSetter
) : Controller
{
    private readonly IGuestService _guestService = guestService;
    private readonly IAuthCookieSetter _authCookieSetter = authCookieSetter;
    private readonly IAuthService _authService = authService;

    [HttpPost("refresh", Name = nameof(Refresh))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ApiProblemDetails>(StatusCodes.Status403Forbidden)]
    [Authorize(AuthPolicies.RefreshAccess)]
    public async Task<ActionResult> Refresh(CancellationToken token = default)
    {
        var result = await _authService.RefreshTokenAsync(HttpContext.User, token);
        return result.Match(
            (value) =>
            {
                _authCookieSetter.SetAuthCookies(
                    value.AccessToken,
                    value.RefreshToken,
                    HttpContext
                );
                return NoContent();
            },
            (errors) => errors.ToActionResult()
        );
    }

    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public ActionResult Logout()
    {
        _authCookieSetter.RemoveAuthCookies(HttpContext);
        return NoContent();
    }

    [HttpPost("guest")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public ActionResult CreateGuestUser()
    {
        var guestToken = _guestService.CreateGuestUser();
        _guestService.SetGuestCookie(guestToken, HttpContext);
        return NoContent();
    }

#if DEBUG
    [HttpPost("test-auth")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [Authorize]
    public ActionResult TestAuthed() => NoContent();

    [HttpPost("test-guest-auth")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [Authorize(AuthPolicies.ActiveSession)]
    public ActionResult TestGuest() => NoContent();
#endif
}
