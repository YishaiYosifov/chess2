using Chess2.Api.Extensions;
using Chess2.Api.Models.DTOs;
using Chess2.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Chess2.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController(IUserService userService, IAuthService authService) : ControllerBase
{
    private readonly IUserService _userService = userService;
    private readonly IAuthService _authService = authService;

    [HttpGet("authed")]
    [ProducesResponseType<PrivateUserOut>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [Authorize]
    public async Task<IResult> GetAuthedUser(CancellationToken cancellation)
    {
        var result = await _authService.GetLoggedInUserAsync(HttpContext, cancellation);
        return result.Match(
            (value) => Results.Ok(new PrivateUserOut(value)),
            (errors) => errors.ToProblemDetails());
    }

    [HttpGet("{username}")]
    [ProducesResponseType<UserOut>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IResult> GetUser(string username, CancellationToken cancellation)
    {
        var result = await _userService.GetUserByUsernameAsync(username);
        return result.Match(
            (value) => Results.Ok(new UserOut(value)),
            (errors) => errors.ToProblemDetails());
    }
}
