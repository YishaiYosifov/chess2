using Chess2.Api.Extensions;
using Chess2.Api.Models.DTOs;
using Chess2.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Chess2.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(ILogger<AuthController> logger, IAuthService authService) : Controller
{
    private readonly ILogger<AuthController> _logger = logger;
    private readonly IAuthService _authService = authService;

    [HttpPost("register")]
    [ProducesResponseType<PrivateUserOut>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IResult> Register([FromBody] UserIn userIn, CancellationToken cancellation)
    {
        throw new Exception();
        var result = await _authService.RegisterUserAsync(userIn, cancellation);
        return result.Match((value) =>
        {
            _logger.LogInformation("Created user {Username}", userIn.Username);
            return Results.Ok(new PrivateUserOut(value));
        }, (errors) => errors.ToProblemDetails());
    }

    [HttpPost("login")]
    [ProducesResponseType<Tokens>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IResult> Login([FromBody] UserLogin userAuth, CancellationToken cancellation)
    {
        var result = await _authService.LoginUserAsync(userAuth, cancellation);
        return result.Match((value) =>
        {
            _authService.SetAccessCookie(value.AccessToken, HttpContext);
            _authService.SetRefreshCookie(value.RefreshToken, HttpContext);
            _logger.LogInformation(
                "User logged in with username/email {UsernameOrEmail}",
                userAuth.UsernameOrEmail);
            return Results.Ok(value);
        }, (errors) => errors.ToProblemDetails());
    }

    [HttpPost("refresh")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [Authorize("RefreshToken")]
    public async Task<IResult> Refresh(CancellationToken cancellation)
    {
        var result = await _authService.RefreshTokenAsync(HttpContext, cancellation);
        return result.Match((value) =>
        {
            _authService.SetAccessCookie(value, HttpContext);
            return Results.NoContent();
        }, (errors) => errors.ToProblemDetails());
    }

#if DEBUG
    [HttpPost("test")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [Authorize]
    public IResult Test() => Results.NoContent();
#endif
}
