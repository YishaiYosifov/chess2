using Chess2.Api.Errors;
using Chess2.Api.Extensions;
using Chess2.Api.Models.DTOs;
using Chess2.Api.Models.Entities;
using Chess2.Api.Repositories;
using ErrorOr;
using FluentValidation;

namespace Chess2.Api.Services;

public interface IUserService
{
    Task<ErrorOr<User>> GetUserByUsernameAsync(string username, CancellationToken cancellation = default);
    Task<ErrorOr<User>> EditProfileAsync(User user, ProfileEdit userEdit, CancellationToken cancellation = default);
}

public class UserService(IValidator<ProfileEdit> userEditValidator, IUserRepository userRepository) : IUserService
{
    private readonly IValidator<ProfileEdit> _profileEditValidator = userEditValidator;
    private readonly IUserRepository _userRepository = userRepository;

    /// <summary>
    /// Gets the user by its username.
    /// If it was not found, return a not found error
    /// </summary>
    public async Task<ErrorOr<User>> GetUserByUsernameAsync(string username, CancellationToken cancellation = default)
    {
        var user = await _userRepository.GetByUsernameAsync(username, cancellation);
        return user is null ? UserErrors.UserNotFound : user;
    }

    public async Task<ErrorOr<User>> EditProfileAsync(User user, ProfileEdit userEdit, CancellationToken cancellation = default)
    {
        var validationResult = _profileEditValidator.Validate(userEdit);
        if (!validationResult.IsValid)
            return validationResult.Errors.ToErrorList();

        return await _userRepository.EditProfileAsync(user, userEdit, cancellation);
    }
}
