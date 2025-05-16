using Chess2.Api.Errors;
using Chess2.Api.Extensions;
using Chess2.Api.Services.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Chess2.Api.Controllers;

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
    [Authorize("RefreshToken")]
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

    [HttpPost("logout", Name = nameof(Logout))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public ActionResult Logout()
    {
        _authCookieSetter.RemoveAuthCookies(HttpContext);
        return NoContent();
    }

    [HttpPost("guest", Name = nameof(CreateGuestUser))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public ActionResult CreateGuestUser()
    {
        var guestToken = _guestService.CreateGuestUser();
        _guestService.SetGuestCookie(guestToken, HttpContext);
        return NoContent();
    }

#if DEBUG
    [HttpPost("test-auth", Name = nameof(TestAuthed))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [Authorize]
    public ActionResult TestAuthed() => NoContent();

    [HttpPost("test-guest-auth", Name = nameof(TestGuest))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [Authorize("GuestAccess")]
    public ActionResult TestGuest() => NoContent();
#endif
}
