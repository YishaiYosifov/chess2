using Chess2.Api.Extensions;
using Chess2.Api.Models;
using Chess2.Api.Models.DTOs;
using Chess2.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
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
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [Authorize]
    public async Task<IActionResult> GetAuthedUser()
    {
        var result = await _authService.GetLoggedInUserAsync(HttpContext.User);
        return result.Match(
            (value) => Ok(new PrivateUserOut(value)),
            (errors) => errors.ToProblemDetails()
        );
    }

    [HttpGet("by-username/{username}")]
    [ProducesResponseType<UserOut>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
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
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [Authorize]
    public async Task<IActionResult> EditProfileSettings(
        JsonPatchDocument<ProfileEditRequest> profileEditRequest
    )
    {
        var userResult = await _authService.GetLoggedInUserAsync(HttpContext.User);
        if (userResult.IsError)
            return userResult.Errors.ToProblemDetails();

        var editResult = await _userService.EditProfileAsync(userResult.Value, profileEditRequest);
        return editResult.Match((value) => NoContent(), (errors) => errors.ToProblemDetails());
    }

    [HttpPut("edit-username")]
    [ProducesResponseType<PrivateUserOut>(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [Authorize]
    public async Task<IActionResult> EditUsername([FromBody] string username)
    {
        var userResult = await _authService.GetLoggedInUserAsync(HttpContext.User);
        if (userResult.IsError)
            return userResult.Errors.ToProblemDetails();

        var editResult = await _userService.EditUsernameAsync(userResult.Value, username);
        return editResult.Match((value) => NoContent(), (errors) => errors.ToProblemDetails());
    }
}
