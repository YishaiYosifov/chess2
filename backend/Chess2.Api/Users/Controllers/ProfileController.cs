using Chess2.Api.Auth.Services;
using Chess2.Api.Infrastructure;
using Chess2.Api.Infrastructure.Errors;
using Chess2.Api.Infrastructure.Extensions;
using Chess2.Api.Users.DTOs;
using Chess2.Api.Users.Entities;
using Chess2.Api.Users.Errors;
using Chess2.Api.Users.Services;
using ErrorOr;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Chess2.Api.Users.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProfileController(
    IUserService userService,
    IAuthService authService,
    IGuestService guestService,
    UserManager<AuthedUser> userManager
) : ControllerBase
{
    private readonly IUserService _userService = userService;
    private readonly IAuthService _authService = authService;
    private readonly IGuestService _guestService = guestService;
    private readonly UserManager<AuthedUser> _userManager = userManager;

    [HttpGet("me", Name = nameof(GetSessionUser))]
    [ProducesResponseType<SessionUser>(StatusCodes.Status200OK)]
    [Authorize(AuthPolicies.ActiveSession)]
    public async Task<ActionResult<SessionUser>> GetSessionUser()
    {
        var idResult = _authService.GetUserId(User);
        if (idResult.IsError)
            return idResult.Errors.ToActionResult();
        var id = idResult.Value;

        if (_guestService.IsGuest(User))
            return Ok(new GuestUser(id));

        var user = await _userManager.GetUserAsync(HttpContext.User);
        if (user is null)
            return Error.Unauthorized().ToActionResult();

        PrivateUser dto = PrivateUser.FromAuthed(user);
        return Ok(dto);
    }

    [HttpGet("by-username/{username}", Name = nameof(GetUser))]
    [ProducesResponseType<PublicUser>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PublicUser>> GetUser(string username)
    {
        var result = await _userManager.FindByNameAsync(username);
        if (result is null)
            return UserErrors.NotFound.ToActionResult();

        PublicUser dto = PublicUser.FromAuthed(result);
        return Ok(dto);
    }

    [HttpPut("edit-profile", Name = nameof(EditProfileSettings))]
    [ProducesResponseType<PublicUser>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [Authorize]
    public async Task<ActionResult<PublicUser>> EditProfileSettings(
        ProfileEditRequest profileEditRequest
    )
    {
        var user = await _userManager.GetUserAsync(HttpContext.User);
        if (user is null)
            return Error.Unauthorized().ToActionResult();

        var editResult = await _userService.EditProfileAsync(user, profileEditRequest);
        return editResult.Match((value) => NoContent(), (errors) => errors.ToActionResult());
    }

    [HttpPut("edit-username", Name = nameof(EditUsername))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [Authorize]
    public async Task<ActionResult> EditUsername([FromBody] string username)
    {
        var user = await _userManager.GetUserAsync(HttpContext.User);
        if (user is null)
            return Error.Unauthorized().ToActionResult();

        var editResult = await _userService.EditUsernameAsync(user, username);
        return editResult.Match((value) => NoContent(), (errors) => errors.ToActionResult());
    }
}
