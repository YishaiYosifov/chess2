using Chess2.Api.Errors;
using Chess2.Api.Extensions;
using Chess2.Api.Models.DTOs;
using Chess2.Api.Services;
using Chess2.Api.Services.Auth;
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

    [HttpGet("me", Name = nameof(GetAuthedUser))]
    [ProducesResponseType<PrivateUserOut>(StatusCodes.Status200OK)]
    [Authorize]
    public async Task<ActionResult<PrivateUserOut>> GetAuthedUser()
    {
        var result = await _authService.GetLoggedInUserAsync(HttpContext.User);
        return result.Match(
            (value) => Ok(new PrivateUserOut(value)),
            (errors) => errors.ToActionResult()
        );
    }

    [HttpGet("by-username/{username}", Name = nameof(GetUser))]
    [ProducesResponseType<UserOut>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserOut>> GetUser(string username)
    {
        var result = await _userService.GetUserByUsernameAsync(username);
        return result.Match((value) => Ok(new UserOut(value)), (errors) => errors.ToActionResult());
    }

    [HttpPatch("edit-profile", Name = nameof(EditProfileSettings))]
    [ProducesResponseType<PrivateUserOut>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [Authorize]
    public async Task<ActionResult<PrivateUserOut>> EditProfileSettings(
        JsonPatchDocument<ProfileEditRequest> profileEditRequest
    )
    {
        var userResult = await _authService.GetLoggedInUserAsync(HttpContext.User);
        if (userResult.IsError)
            return userResult.Errors.ToActionResult();

        var editResult = await _userService.EditProfileAsync(userResult.Value, profileEditRequest);
        return editResult.Match((value) => NoContent(), (errors) => errors.ToActionResult());
    }

    [HttpPut("edit-username", Name = nameof(EditUsername))]
    [ProducesResponseType<PrivateUserOut>(StatusCodes.Status204NoContent)]
    [Authorize]
    public async Task<ActionResult<PrivateUserOut>> EditUsername([FromBody] string username)
    {
        var userResult = await _authService.GetLoggedInUserAsync(HttpContext.User);
        if (userResult.IsError)
            return userResult.Errors.ToActionResult();

        var editResult = await _userService.EditUsernameAsync(userResult.Value, username);
        return editResult.Match((value) => NoContent(), (errors) => errors.ToActionResult());
    }
}
