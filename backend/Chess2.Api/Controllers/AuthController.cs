using Chess2.Api.Errors;
using Chess2.Api.Extensions;
using Chess2.Api.Models.DTOs;
using Chess2.Api.Repositories;
using Chess2.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Chess2.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(ILogger<AuthController> logger, IUserService userService) : Controller
{
    private readonly IUserService _userService = userService;
    private readonly ILogger<AuthController> _logger = logger;

    [HttpPost("register")]
    [ProducesResponseType(typeof(UserOut), StatusCodes.Status200OK)]
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
}
