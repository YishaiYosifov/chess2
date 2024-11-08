using Chess2.Api.Models.DTOs;
using Chess2.Api.Models.Entities;
using Chess2.Api.Repositories;
using FluentValidation;
using ErrorOr;
using Chess2.Api.Extensions;
using Chess2.Api.Errors;

namespace Chess2.Api.Services;

public interface IUserService
{
    Task<ErrorOr<User>> RegisterUser(UserIn user, CancellationToken cancellation);
}

public class UserService(IValidator<UserIn> userValidator, IUserRepository userRepository, IPasswordHasher passwordHasher) : IUserService
{
    private readonly IValidator<UserIn> _userValidator = userValidator;
    private readonly IUserRepository _userRepository = userRepository;
    private readonly IPasswordHasher _passwordHasher = passwordHasher;

    /// <summary>
    /// Register a new user
    /// </summary>
    /// <param name="user">The user DTO received from the client</param>
    public async Task<ErrorOr<User>> RegisterUser(UserIn user, CancellationToken cancellation)
    {
        var validationResult = _userValidator.Validate(user);
        if (!validationResult.IsValid)
            return validationResult.Errors.ToErrorList();

        // make sure there are no conflicts
        var conflictErrors = new List<Error>();
        if (await _userRepository.IsUsernameTaken(user.Username, cancellation))
            conflictErrors.Add(UserErrors.UsernameTaken);
        if (await _userRepository.IsEmailTaken(user.Email, cancellation))
            conflictErrors.Add(UserErrors.EmailTaken);

        if (conflictErrors.Count != 0) return conflictErrors;

        // create the user
        var salt = _passwordHasher.GenerateSalt();
        var hash = await _passwordHasher.HashPassword(user.Password, salt);

        var dbUser = new User()
        {
            Username = user.Username,
            Email = user.Email,
            PasswordHash = hash,
            PasswordSalt = salt,
        };
        await _userRepository.AddUser(dbUser, cancellation);

        return dbUser;
    }
}
