using Chess2.Api.Auth.Services;
using Chess2.Api.Infrastructure;
using Chess2.Api.Infrastructure.Errors;
using Chess2.Api.Infrastructure.Extensions;
using Chess2.Api.Profile.DTOs;
using Chess2.Api.Profile.Entities;
using Chess2.Api.Profile.Errors;
using Chess2.Api.Profile.Services;
using ErrorOr;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Chess2.Api.Profile.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProfileController(
    IValidator<ProfileEditRequest> profileEditValidator,
    IValidator<UsernameEditRequest> usernameEditValidator,
    IProfileSettings profileSettings,
    IAuthService authService,
    IGuestService guestService,
    UserManager<AuthedUser> userManager,
    IProfilePictureProvider profilePictureProvider
) : Controller
{
    private readonly IValidator<ProfileEditRequest> _profileEditValidator = profileEditValidator;
    private readonly IValidator<UsernameEditRequest> _usernameEditValidator = usernameEditValidator;
    private readonly IProfileSettings _profileSettings = profileSettings;
    private readonly IAuthService _authService = authService;
    private readonly IGuestService _guestService = guestService;
    private readonly UserManager<AuthedUser> _userManager = userManager;
    private readonly IProfilePictureProvider _profilePictureProvider = profilePictureProvider;

    [HttpGet("me")]
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

        PrivateUser dto = new(user);
        return Ok(dto);
    }

    [HttpGet("by-username/{username}")]
    [ProducesResponseType<PublicUser>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PublicUser>> GetUserByUsername(string username)
    {
        var result = await _userManager.FindByNameAsync(username);
        if (result is null)
            return ProfileErrors.NotFound.ToActionResult();

        PublicUser dto = new(result);
        return Ok(dto);
    }

    [HttpGet("by-id/{userId}")]
    [ProducesResponseType<PublicUser>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PublicUser>> GetUserById(string userId)
    {
        var result = await _userManager.FindByIdAsync(userId);
        if (result is null)
            return ProfileErrors.NotFound.ToActionResult();

        PublicUser dto = new(result);
        return Ok(dto);
    }

    [HttpPut("edit-profile")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [Authorize]
    public async Task<ActionResult> EditProfileSettings(ProfileEditRequest profileEditRequest)
    {
        var validationResult = _profileEditValidator.Validate(profileEditRequest);
        if (!validationResult.IsValid)
            return validationResult.Errors.ToErrorList().ToActionResult();

        var user = await _userManager.GetUserAsync(HttpContext.User);
        if (user is null)
            return Error.Unauthorized().ToActionResult();

        var editResult = await _profileSettings.EditProfileAsync(user, profileEditRequest);
        return editResult.Match((value) => NoContent(), (errors) => errors.ToActionResult());
    }

    [HttpPut("edit-username")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [Authorize]
    public async Task<ActionResult> EditUsername(UsernameEditRequest usernameEditRequest)
    {
        var validationResult = _usernameEditValidator.Validate(usernameEditRequest);
        if (!validationResult.IsValid)
            return validationResult.Errors.ToErrorList().ToActionResult();

        var user = await _userManager.GetUserAsync(HttpContext.User);
        if (user is null)
            return Error.Unauthorized().ToActionResult();

        var editResult = await _profileSettings.EditUsernameAsync(
            user,
            usernameEditRequest.Username
        );
        return editResult.Match(value => NoContent(), errors => errors.ToActionResult());
    }

    [HttpPut("profile-picture")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [RequestSizeLimit(2 * 1024 * 1024)]
    [Authorize]
    public async Task<ActionResult> UploadProfilePicture(
        [FromForm] UploadPfpRequest form,
        CancellationToken token
    )
    {
        var userIdResult = _authService.GetUserId(HttpContext.User);
        if (userIdResult.IsError)
            return userIdResult.Errors.ToActionResult();

        using var stream = form.File.OpenReadStream();
        var uploadResult = await _profilePictureProvider.UploadProfilePictureAsync(
            userIdResult.Value,
            stream,
            token
        );
        return uploadResult.Match(value => Created(), errors => errors.ToActionResult());
    }

    [HttpDelete("profile-picture")]
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

    [HttpGet("profile-picture/{userId}")]
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
