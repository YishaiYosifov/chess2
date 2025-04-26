using Chess2.Api.Errors;
using Chess2.Api.Extensions;
using Chess2.Api.Models;
using Chess2.Api.Models.DTOs;
using Chess2.Api.Models.Entities;
using ErrorOr;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Chess2.Api.Services;

public interface IUserService
{
    Task<ErrorOr<AuthedUser>> GetUserByUsernameAsync(string username);
    Task<ErrorOr<Updated>> EditProfileAsync(AuthedUser user, ProfileEdit userEdit);
    Task<ErrorOr<Updated>> EditUsernameAsync(AuthedUser user, string username);
}

public class UserService(
    IValidator<ProfileEdit> userEditValidator,
    UserManager<AuthedUser> userManager,
    IOptions<AppSettings> settings,
    ILogger<UserService> logger
) : IUserService
{
    private readonly IValidator<ProfileEdit> _profileEditValidator = userEditValidator;
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
        return user is null ? UserErrors.UserNotFound : user;
    }

    public async Task<ErrorOr<Updated>> EditProfileAsync(AuthedUser user, ProfileEdit userEdit)
    {
        var validationResult = _profileEditValidator.Validate(userEdit);
        if (!validationResult.IsValid)
            return validationResult.Errors.ToErrorList();

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
