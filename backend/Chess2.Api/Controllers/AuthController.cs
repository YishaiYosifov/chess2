using Chess2.Api.Errors;
using Chess2.Api.Extensions;
using Chess2.Api.Services.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Chess2.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(
    ILogger<AuthController> logger,
    IAuthService authService,
    IOAuthService oAuthService,
    IGuestService guestService,
    IAuthCookieSetter authCookieSetter
) : Controller
{
    private readonly IGuestService _guestService = guestService;
    private readonly IAuthCookieSetter _authCookieSetter = authCookieSetter;
    private readonly ILogger<AuthController> _logger = logger;
    private readonly IAuthService _authService = authService;
    private readonly IOAuthService _oAuthService = oAuthService;

    [HttpGet("signin/google", Name = nameof(SigninGoogle))]
    [ProducesResponseType(StatusCodes.Status302Found)]
    public ActionResult SigninGoogle([FromQuery] string returnUrl)
    {
        var properties = _oAuthService.ConfigureGoogleOAuthProperties(returnUrl, HttpContext);
        return Challenge(properties, ["Google"]);
    }

    [HttpGet("signin/google/callback", Name = nameof(SigninGoogleCallback))]
    public async Task<ActionResult> SigninGoogleCallback([FromQuery] string returnUrl)
    {
        var result = await _oAuthService.AuthenticateGoogleAsync(HttpContext);
        return result.Match(value => Redirect(returnUrl), errors => errors.ToActionResult());
    }

    [HttpPost("refresh", Name = nameof(Refresh))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ApiProblemDetails>(StatusCodes.Status403Forbidden)]
    [Authorize("RefreshToken")]
    public async Task<ActionResult> Refresh()
    {
        var result = await _authService.RefreshTokenAsync(HttpContext.User);
        return result.Match(
            (value) =>
            {
                _authCookieSetter.SetAccessCookie(value.newTokens.AccessToken, HttpContext);
                _authCookieSetter.SetIsAuthedCookie(HttpContext);
                return NoContent();
            },
            (errors) =>
            {
                _authService.Logout(HttpContext);
                return errors.ToActionResult();
            }
        );
    }

    [HttpPost("logout", Name = nameof(Logout))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public ActionResult Logout()
    {
        _authService.Logout(HttpContext);
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
    [HttpPost("test-authed", Name = nameof(TestAuthed))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [Authorize]
    public ActionResult TestAuthed() => NoContent();

    [HttpPost("test-guest", Name = nameof(TestGuest))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [Authorize("GuestAccess")]
    public ActionResult TestGuest() => NoContent();
#endif
}
