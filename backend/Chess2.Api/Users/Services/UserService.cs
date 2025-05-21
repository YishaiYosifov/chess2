using Chess2.Api.Infrastructure.Extensions;
using Chess2.Api.Shared.Models;
using Chess2.Api.Users.DTOs;
using Chess2.Api.Users.Entities;
using Chess2.Api.Users.Errors;
using ErrorOr;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.Extensions.Options;

namespace Chess2.Api.Users.Services;

public interface IUserService
{
    Task<ErrorOr<AuthedUser>> GetUserByUsernameAsync(string username);
    Task<ErrorOr<Updated>> EditProfileAsync(
        AuthedUser user,
        JsonPatchDocument<ProfileEditRequest> profileEdit
    );
    Task<ErrorOr<Updated>> EditUsernameAsync(AuthedUser user, string username);
}

public class UserService(
    IValidator<ProfileEditRequest> userEditValidator,
    UserManager<AuthedUser> userManager,
    IOptions<AppSettings> settings,
    ILogger<UserService> logger
) : IUserService
{
    private readonly IValidator<ProfileEditRequest> _profileEditValidator = userEditValidator;
    private readonly UserManager<AuthedUser> _userManager = userManager;
    private readonly AppSettings _settings = settings.Value;
    private readonly ILogger<UserService> _logger = logger;

    /// <summary>
    /// Gets the user by its username.
    /// If it was not found, return a not found error
    /// </summary>
    public async Task<ErrorOr<AuthedUser>> GetUserByUsernameAsync(string username)
    {
        var user = await _userManager.FindByNameAsync(username);
        return user is null ? UserErrors.NotFound : user;
    }

    public async Task<ErrorOr<Updated>> EditProfileAsync(
        AuthedUser user,
        JsonPatchDocument<ProfileEditRequest> profileEdit
    )
    {
        var applyToProfile = new ProfileEditRequest(user);
        profileEdit.ApplyTo(applyToProfile);

        var validationResult = _profileEditValidator.Validate(applyToProfile);
        if (!validationResult.IsValid)
            return validationResult.Errors.ToErrorList();

        user.About = applyToProfile.About;
        user.CountryCode = applyToProfile.CountryCode;

        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            _logger.LogWarning("Failed to edit user {UserId}", user.Id);
            return updateResult.Errors.ToErrorList();
        }

        return Result.Updated;
    }

    public async Task<ErrorOr<Updated>> EditUsernameAsync(AuthedUser user, string username)
    {
        if (DateTime.UtcNow - user.UsernameLastChanged < _settings.UsernameEditCooldown)
            return UserErrors.SettingOnCooldown;

        var updateResult = await _userManager.SetUserNameAsync(user, username);
        if (!updateResult.Succeeded)
        {
            _logger.LogWarning("Failed to edit user {UserId}", user.Id);
            return updateResult.Errors.ToErrorList();
        }

        return Result.Updated;
    }
}
