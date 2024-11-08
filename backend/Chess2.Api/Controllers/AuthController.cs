using Chess2.Api.Extensions;
using Chess2.Api.Models.DTOs;
using Chess2.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Chess2.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(ILogger<AuthController> logger, IUserService userService) : Controller
{
    private readonly IUserService _userService = userService;
    private readonly ILogger<AuthController> _logger = logger;

    [HttpPost("register")]
    [ProducesResponseType<UserOut>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IResult> Register([FromBody] UserIn userIn, CancellationToken cancellation)
    {
        var result = await _userService.RegisterUserAsync(userIn, cancellation);
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
        var result = await _userService.LoginUserAsync(userAuth, cancellation);
        return result.Match((value) =>
        {
            _logger.LogInformation(
                "User logged in with username/email {UsernameOrEmail}",
                userAuth.UsernameOrEmail);
            return Results.Ok(result.Value);
        }, (errors) => errors.ToProblemDetails());
    }

    [HttpPost("test")]
    [Authorize]
    public async Task Test(CancellationToken cancellation)
    {

    }
}
