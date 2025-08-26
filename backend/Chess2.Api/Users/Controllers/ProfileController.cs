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
    IUserSettings userSettings,
    IAuthService authService,
    IGuestService guestService,
    UserManager<AuthedUser> userManager,
    IProfilePictureProvider profilePictureProvider
) : ControllerBase
{
    private readonly IUserSettings _userSettings = userSettings;
    private readonly IAuthService _authService = authService;
    private readonly IGuestService _guestService = guestService;
    private readonly UserManager<AuthedUser> _userManager = userManager;
    private readonly IProfilePictureProvider _profilePictureProvider = profilePictureProvider;

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
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [Authorize]
    public async Task<ActionResult> EditProfileSettings(ProfileEditRequest profileEditRequest)
    {
        var user = await _userManager.GetUserAsync(HttpContext.User);
        if (user is null)
            return Error.Unauthorized().ToActionResult();

        var editResult = await _userSettings.EditProfileAsync(user, profileEditRequest);
        return editResult.Match((value) => NoContent(), (errors) => errors.ToActionResult());
    }

    [HttpPut("edit-username", Name = nameof(EditUsername))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [Authorize]
    public async Task<ActionResult> EditUsername(UsernameEditRequest usernameEditRequest)
    {
        var user = await _userManager.GetUserAsync(HttpContext.User);
        if (user is null)
            return Error.Unauthorized().ToActionResult();

        var editResult = await _userSettings.EditUsernameAsync(user, usernameEditRequest);
        return editResult.Match(value => NoContent(), errors => errors.ToActionResult());
    }

    [HttpPut("profile-picture", Name = nameof(UploadProfilePicture))]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [RequestSizeLimit(2 * 1024 * 1024)]
    [Authorize]
    public async Task<ActionResult> UploadProfilePicture(
        [FromForm] IFormFile file,
        CancellationToken token
    )
    {
        var userIdResult = _authService.GetUserId(HttpContext.User);
        if (userIdResult.IsError)
            return userIdResult.Errors.ToActionResult();

        using var stream = file.OpenReadStream();
        var uploadResult = await _profilePictureProvider.UploadProfilePictureAsync(
            userIdResult.Value,
            stream,
            token
        );
        return uploadResult.Match(value => Created(), errors => errors.ToActionResult());
    }

    [HttpDelete("profile-picture", Name = nameof(DeleteProfilePicture))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [Authorize]
    public async Task<ActionResult> DeleteProfilePicture(CancellationToken token)
    {
        var userIdResult = _authService.GetUserId(HttpContext.User);
        if (userIdResult.IsError)
            return userIdResult.Errors.ToActionResult();

        await _profilePictureProvider.DeleteProfilePictureAsync(userIdResult.Value, token);
        return NoContent();
    }

    [HttpGet("profile-picture/{userId}", Name = nameof(GetProfilePicture))]
    [ProducesResponseType<FileResult>(StatusCodes.Status200OK)]
    public async Task<ActionResult> GetProfilePicture(string userId, CancellationToken token)
    {
        var lastModified = await _profilePictureProvider.GetLastModifiedAsync(userId, token);
        var etag = $"\"{lastModified.Ticks}\"";
        Response.Headers.ETag = etag;

        if (HttpContext.Request.Headers.TryGetValue("If-None-Match", out var inm) && inm == etag)
            return StatusCode(StatusCodes.Status304NotModified);

        var image = await _profilePictureProvider.GetProfilePictureAsync(userId, token);
        return File(image, "image/webp");
    }
}
