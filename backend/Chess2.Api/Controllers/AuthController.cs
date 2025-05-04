using Chess2.Api.Errors;
using Chess2.Api.Extensions;
using Chess2.Api.Models.DTOs;
using Chess2.Api.Services.Auth;
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
    IAuthCookieSetter authCookieSetter,
    IValidator<SignupRequest> userValidator
) : Controller
{
    private readonly IGuestService _guestService = guestService;
    private readonly IAuthCookieSetter _authCookieSetter = authCookieSetter;
    private readonly ILogger<AuthController> _logger = logger;
    private readonly IAuthService _authService = authService;
    private readonly IValidator<SignupRequest> _signupValidator = userValidator;

    [HttpPost("signup", Name = nameof(Signup))]
    [ProducesResponseType<PrivateUserOut>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ApiProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<PrivateUserOut>> Signup([FromBody] SignupRequest signupRequest)
    {
        var validateResults = await _signupValidator.ValidateAsync(signupRequest);
        if (!validateResults.IsValid)
            return validateResults.Errors.ToErrorList().ToActionResult();

        var result = await _authService.SignupAsync(signupRequest);
        return result.Match(
            (value) =>
            {
                _logger.LogInformation("Created user {Username}", signupRequest.UserName);
                return Ok(new PrivateUserOut(value));
            },
            (errors) => errors.ToActionResult()
        );
    }

    [HttpPost("signin", Name = nameof(Signin))]
    [ProducesResponseType<AuthResponseDTO>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AuthResponseDTO>> Signin([FromBody] SigninRequest signinRequest)
    {
        var result = await _authService.SigninAsync(
            signinRequest.UsernameOrEmail,
            signinRequest.Password
        );
        return result.Match(
            (value) =>
            {
                _authCookieSetter.SetAccessCookie(value.tokens.AccessToken, HttpContext);
                _authCookieSetter.SetRefreshCookie(value.tokens.RefreshToken, HttpContext);
                _logger.LogInformation(
                    "User logged in with username/email {UsernameOrEmail}",
                    signinRequest.UsernameOrEmail
                );
                return Ok(new AuthResponseDTO() { AuthTokens = value.tokens, User = value.user });
            },
            (errors) => errors.ToActionResult()
        );
    }

    [HttpPost("refresh", Name = nameof(Refresh))]
    [ProducesResponseType<AuthResponseDTO>(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ApiProblemDetails>(StatusCodes.Status403Forbidden)]
    [Authorize("RefreshToken")]
    public async Task<ActionResult<AuthResponseDTO>> Refresh()
    {
        var result = await _authService.RefreshTokenAsync(HttpContext.User);
        return result.Match(
            (value) =>
            {
                _authCookieSetter.SetAccessCookie(value.newTokens.AccessToken, HttpContext);
                return Ok(
                    new AuthResponseDTO() { AuthTokens = value.newTokens, User = value.user }
                );
            },
            (errors) => errors.ToActionResult()
        );
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
