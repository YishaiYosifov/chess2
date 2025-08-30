using Chess2.Api.Infrastructure.Extensions;
using Chess2.Api.Shared.Models;
using Chess2.Api.Profile.DTOs;
using Chess2.Api.Profile.Entities;
using Chess2.Api.Profile.Errors;
using ErrorOr;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Chess2.Api.Profile.Services;

public interface IUserSettings
{
    Task<ErrorOr<Updated>> EditProfileAsync(AuthedUser user, ProfileEditRequest profileEdit);
    Task<ErrorOr<Updated>> EditUsernameAsync(AuthedUser user, UsernameEditRequest usernameEdit);
}

public class UserSettings(
    IValidator<ProfileEditRequest> profileEditValidator,
    IValidator<UsernameEditRequest> usernameEditValidator,
    UserManager<AuthedUser> userManager,
    IOptions<AppSettings> settings,
    ILogger<UserSettings> logger,
    TimeProvider timeProvider
) : IUserSettings
{
    private readonly IValidator<ProfileEditRequest> _profileEditValidator = profileEditValidator;
    private readonly IValidator<UsernameEditRequest> _usernameEditValidator = usernameEditValidator;
    private readonly UserManager<AuthedUser> _userManager = userManager;
    private readonly AppSettings _settings = settings.Value;
    private readonly ILogger<UserSettings> _logger = logger;
    private readonly TimeProvider _timeProvider = timeProvider;

    public async Task<ErrorOr<Updated>> EditProfileAsync(
        AuthedUser user,
        ProfileEditRequest profileEdit
    )
    {
        var validationResult = _profileEditValidator.Validate(profileEdit);
        if (!validationResult.IsValid)
            return validationResult.Errors.ToErrorList();

        profileEdit.ApplyTo(user);
        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            _logger.LogWarning("Failed to edit user {UserId}", user.Id);
            return updateResult.Errors.ToErrorList();
        }

        return Result.Updated;
    }

    public async Task<ErrorOr<Updated>> EditUsernameAsync(
        AuthedUser user,
        UsernameEditRequest usernameEdit
    )
    {
        var validationResult = _usernameEditValidator.Validate(usernameEdit);
        if (!validationResult.IsValid)
            return validationResult.Errors.ToErrorList();

        var now = _timeProvider.GetUtcNow().UtcDateTime;
        if (now - user.UsernameLastChanged < _settings.UsernameEditCooldown)
            return UserErrors.SettingOnCooldown;

        var updateResult = await _userManager.SetUserNameAsync(user, usernameEdit.Username);
        if (!updateResult.Succeeded)
        {
            _logger.LogWarning("Failed to edit user {UserId}", user.Id);
            return updateResult.Errors.ToErrorList();
        }

        user.UsernameLastChanged = now;
        await _userManager.UpdateAsync(user);

        return Result.Updated;
    }
}
