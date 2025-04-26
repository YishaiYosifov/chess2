using Chess2.Api.Extensions;
using Chess2.Api.Models.DTOs;
using Chess2.Api.Services;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Chess2.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(
    ILogger<AuthController> logger,
    IAuthService authService,
    IGuestService guestService,
    IValidator<SignupRequest> userValidator
) : Controller
{
    private readonly IGuestService _guestService = guestService;
    private readonly ILogger<AuthController> _logger = logger;
    private readonly IAuthService _authService = authService;
    private readonly IValidator<SignupRequest> _signupValidator = userValidator;

    [HttpPost("signup")]
    [ProducesResponseType<PrivateUserOut>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Signup(
        [FromBody] SignupRequest signupRequest,
        CancellationToken cancellation
    )
    {
        var validateResults = await _signupValidator.ValidateAsync(signupRequest, cancellation);
        if (!validateResults.IsValid)
            return validateResults.Errors.ToErrorList().ToProblemDetails();

        var result = await _authService.SignupAsync(signupRequest, cancellation);
        return result.Match(
            (value) =>
            {
                _logger.LogInformation("Created user {Username}", signupRequest.UserName);
                return Ok(new PrivateUserOut(value));
            },
            (errors) => errors.ToProblemDetails()
        );
    }

    [HttpPost("signin")]
    [ProducesResponseType<Tokens>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Login([FromBody] SigninRequest signinRequest)
    {
        var result = await _authService.SigninAsync(
            signinRequest.UsernameOrEmail,
            signinRequest.Password
        );
        return result.Match(
            (value) =>
            {
                _authService.SetAccessCookie(value.AccessToken, HttpContext);
                _authService.SetRefreshCookie(value.RefreshToken, HttpContext);
                _logger.LogInformation(
                    "User logged in with username/email {UsernameOrEmail}",
                    signinRequest.UsernameOrEmail
                );
                return Ok(value);
            },
            (errors) => errors.ToProblemDetails()
        );
    }

    [HttpPost("refresh")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [Authorize("RefreshToken")]
    public async Task<IActionResult> Refresh(CancellationToken cancellation)
    {
        var result = await _authService.RefreshTokenAsync(HttpContext.User, cancellation);
        return result.Match(
            (value) =>
            {
                _authService.SetAccessCookie(value, HttpContext);
                return NoContent();
            },
            (errors) => errors.ToProblemDetails()
        );
    }

    [HttpPost("guest")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public IActionResult CreatesGuestUser()
    {
        var guestToken = _guestService.CreateGuestUser();
        _guestService.SetGuestCookie(guestToken, HttpContext);
        return NoContent();
    }

#if DEBUG
    [HttpPost("test-authed")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [Authorize]
    public IActionResult TestAuthed() => NoContent();

    [HttpPost("test-guest")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [Authorize("GuestAccess")]
    public IActionResult TestGuest() => NoContent();
#endif
}
