using AnarchyChess.Api.Infrastructure.Extensions;
using AnarchyChess.Api.Profile.DTOs;
using AnarchyChess.Api.Profile.Entities;
using AnarchyChess.Api.Profile.Errors;
using AnarchyChess.Api.Shared.Models;
using ErrorOr;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace AnarchyChess.Api.Profile.Services;

public interface IProfileSettings
{
    Task<ErrorOr<Updated>> EditProfileAsync(AuthedUser user, ProfileEditRequest profileEdit);
    Task<ErrorOr<Updated>> EditUsernameAsync(AuthedUser user, string newUserName);
}

public class ProfileSettings(
    UserManager<AuthedUser> userManager,
    IOptions<AppSettings> settings,
    ILogger<ProfileSettings> logger,
    TimeProvider timeProvider
) : IProfileSettings
{
    private readonly UserManager<AuthedUser> _userManager = userManager;
    private readonly AppSettings _settings = settings.Value;
    private readonly ILogger<ProfileSettings> _logger = logger;
    private readonly TimeProvider _timeProvider = timeProvider;

    public async Task<ErrorOr<Updated>> EditProfileAsync(
        AuthedUser user,
        ProfileEditRequest profileEdit
    )
    {
        profileEdit.ApplyTo(user);
        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            _logger.LogWarning("Failed to edit user {UserId}", user.Id);
            return updateResult.Errors.ToErrorList();
        }

        return Result.Updated;
    }

    public async Task<ErrorOr<Updated>> EditUsernameAsync(AuthedUser user, string newUserName)
    {
        var now = _timeProvider.GetUtcNow().UtcDateTime;
        if (now - user.UsernameLastChanged < _settings.UsernameEditCooldown)
            return ProfileErrors.SettingOnCooldown;

        if (await _userManager.FindByNameAsync(newUserName) is not null)
            return ProfileErrors.UserNameTaken;

        var updateResult = await _userManager.SetUserNameAsync(user, newUserName);
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
