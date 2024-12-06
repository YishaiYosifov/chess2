using Chess2.Api.Extensions;
using Chess2.Api.Models.DTOs;
using Chess2.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Chess2.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(ILogger<AuthController> logger, IAuthService authService, IGuestService guestService) : Controller
{
    private readonly IGuestService _guestService = guestService;
    private readonly ILogger<AuthController> _logger = logger;
    private readonly IAuthService _authService = authService;

    [HttpPost("signup")]
    [ProducesResponseType<PrivateUserOut>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Signup([FromBody] UserIn userIn, CancellationToken cancellation)
    {
        var result = await _authService.SignupUserAsync(userIn, cancellation);
        return result.Match((value) =>
        {
            _logger.LogInformation("Created user {Username}", userIn.Username);
            return Ok(new PrivateUserOut(value));
        }, (errors) => errors.ToProblemDetails());
    }

    [HttpPost("login")]
    [ProducesResponseType<Tokens>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Login([FromBody] UserLogin userAuth, CancellationToken cancellation)
    {
        var result = await _authService.LoginUserAsync(userAuth, cancellation);
        return result.Match((value) =>
        {
            _authService.SetAccessCookie(value.AccessToken, HttpContext);
            _authService.SetRefreshCookie(value.RefreshToken, HttpContext);
            _logger.LogInformation(
                "User logged in with username/email {UsernameOrEmail}",
                userAuth.UsernameOrEmail);
            return Ok(value);
        }, (errors) => errors.ToProblemDetails());
    }

    [HttpPost("refresh")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [Authorize("RefreshToken")]
    public async Task<IActionResult> Refresh(CancellationToken cancellation)
    {
        var result = await _authService.RefreshTokenAsync(HttpContext, cancellation);
        return result.Match((value) =>
        {
            _authService.SetAccessCookie(value, HttpContext);
            return NoContent();
        }, (errors) => errors.ToProblemDetails());
    }

#if DEBUG
    [HttpPost("test")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [Authorize]
    public IActionResult Test() => NoContent();
#endif

    [HttpPost("guest")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public IActionResult CreatesGuestUser()
    {
        var guestToken = _guestService.CreateGuestUser();
        _guestService.SetGuestCookie(guestToken, HttpContext);
        return NoContent();
    }
}
