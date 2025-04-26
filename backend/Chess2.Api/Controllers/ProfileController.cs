using Chess2.Api.Extensions;
using Chess2.Api.Models.DTOs;
using Chess2.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Chess2.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProfileController(IUserService userService, IAuthService authService) : ControllerBase
{
    private readonly IUserService _userService = userService;
    private readonly IAuthService _authService = authService;

    [HttpGet("me")]
    [ProducesResponseType<PrivateUserOut>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [Authorize]
    public async Task<IActionResult> GetAuthedUser(CancellationToken cancellation)
    {
        var result = await _authService.GetLoggedInUserAsync(HttpContext.User, cancellation);
        return result.Match(
            (value) => Ok(new PrivateUserOut(value)),
            (errors) => errors.ToProblemDetails()
        );
    }

    [HttpGet("by-username/{username}")]
    [ProducesResponseType<UserOut>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUser(string username)
    {
        var result = await _userService.GetUserByUsernameAsync(username);
        return result.Match(
            (value) => Ok(new UserOut(value)),
            (errors) => errors.ToProblemDetails()
        );
    }

    [HttpPatch("edit-profile")]
    [ProducesResponseType<PrivateUserOut>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [Authorize]
    public async Task<IActionResult> EditProfileSettings(
        [FromBody] ProfileEdit userEdit,
        CancellationToken cancellation
    )
    {
        var userResult = await _authService.GetLoggedInUserAsync(HttpContext.User, cancellation);
        if (userResult.IsError)
            return userResult.Errors.ToProblemDetails();

        var editResult = await _userService.EditProfileAsync(userResult.Value, userEdit);
        return editResult.Match((value) => NoContent(), (errors) => errors.ToProblemDetails());
    }

    [HttpPut("edit-username")]
    [ProducesResponseType<PrivateUserOut>(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [Authorize]
    public async Task<IActionResult> EditUsername(
        [FromBody] string username,
        CancellationToken cancellation
    )
    {
        var userResult = await _authService.GetLoggedInUserAsync(HttpContext.User, cancellation);
        if (userResult.IsError)
            return userResult.Errors.ToProblemDetails();

        var editResult = await _userService.EditUsernameAsync(userResult.Value, username);
        return editResult.Match((value) => NoContent(), (errors) => errors.ToProblemDetails());
    }
}
