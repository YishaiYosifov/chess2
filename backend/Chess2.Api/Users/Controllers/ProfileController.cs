using Chess2.Api.Infrastructure.Errors;
using Chess2.Api.Infrastructure.Extensions;
using Chess2.Api.Users.DTOs;
using Chess2.Api.Users.Entities;
using Chess2.Api.Users.Services;
using ErrorOr;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace Chess2.Api.Users.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProfileController(IUserService userService, UserManager<AuthedUser> userManager)
    : ControllerBase
{
    private readonly IUserService _userService = userService;

    [HttpGet("me", Name = nameof(GetAuthedUser))]
    [ProducesResponseType<PrivateUserOut>(StatusCodes.Status200OK)]
    [Authorize]
    public async Task<ActionResult<PrivateUserOut>> GetAuthedUser()
    {
        var user = await userManager.GetUserAsync(HttpContext.User);
        if (user is null)
            return Error.Unauthorized().ToActionResult();

        var dto = new PrivateUserOut(user);
        return Ok(dto);
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
        var user = await userManager.GetUserAsync(HttpContext.User);
        if (user is null)
            return Error.Unauthorized().ToActionResult();

        var editResult = await _userService.EditProfileAsync(user, profileEditRequest);
        return editResult.Match((value) => NoContent(), (errors) => errors.ToActionResult());
    }

    [HttpPut("edit-username", Name = nameof(EditUsername))]
    [ProducesResponseType<PrivateUserOut>(StatusCodes.Status204NoContent)]
    [Authorize]
    public async Task<ActionResult<PrivateUserOut>> EditUsername([FromBody] string username)
    {
        var user = await userManager.GetUserAsync(HttpContext.User);
        if (user is null)
            return Error.Unauthorized().ToActionResult();

        var editResult = await _userService.EditUsernameAsync(user, username);
        return editResult.Match((value) => NoContent(), (errors) => errors.ToActionResult());
    }
}
