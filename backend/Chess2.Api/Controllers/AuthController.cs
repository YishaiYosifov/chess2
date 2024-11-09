using Chess2.Api.Extensions;
using Chess2.Api.Models;
using Chess2.Api.Models.DTOs;
using Chess2.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Chess2.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(ILogger<AuthController> logger, IAuthService authService, IOptions<AppSettings> settings) : Controller
{
    private readonly JwtSettings _jwtSettings = settings.Value.Jwt;
    private readonly ILogger<AuthController> _logger = logger;
    private readonly IAuthService _authService = authService;

    [HttpPost("register")]
    [ProducesResponseType<UserOut>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IResult> Register([FromBody] UserIn userIn, CancellationToken cancellation)
    {
        var result = await _authService.RegisterUserAsync(userIn, cancellation);
        return result.Match((value) =>
        {
            _logger.LogInformation("Created user {Username}", userIn.Username);
            return Results.Ok(new UserOut(value));
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
            _authService.SetTokenCookies(value, HttpContext);
            _logger.LogInformation(
                "User logged in with username/email {UsernameOrEmail}",
                userAuth.UsernameOrEmail);
            return Results.Ok();
        }, (errors) => errors.ToProblemDetails());
    }

    [HttpPost("refresh")]
    [Authorize("RefreshToken")]
    public async Task<IResult> Refresh(CancellationToken cancellation)
    {
        var userResult = await _authService.GetLoggedInUser(cancellation);
        if (userResult.IsError) return userResult.Errors.ToProblemDetails();
        var user = userResult.Value;

        return Results.Ok(new UserOut(user));
    }
}
