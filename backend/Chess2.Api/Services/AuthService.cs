using Chess2.Api.Errors;
using Chess2.Api.Extensions;
using Chess2.Api.Models.DTOs;
using Chess2.Api.Models.Entities;
using Chess2.Api.Repositories;
using ErrorOr;
using FluentValidation;

namespace Chess2.Api.Services;

public interface IAuthService
{
    Task<ErrorOr<User>> RegisterUserAsync(UserIn userIn, CancellationToken cancellation);
    Task<ErrorOr<Tokens>> LoginUserAsync(UserLogin userAuth, CancellationToken cancellation);
}

public class AuthService(
    IValidator<UserIn> userValidator,
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    ITokenProvider tokenProvider) : IAuthService
{
    private readonly IValidator<UserIn> _userValidator = userValidator;
    private readonly IUserRepository _userRepository = userRepository;
    private readonly IPasswordHasher _passwordHasher = passwordHasher;
    private readonly ITokenProvider _tokenProvider = tokenProvider;

    /// <summary>
    /// Register a new user
    /// </summary>
    /// <param name="userIn">The user DTO received from the client</param>
    public async Task<ErrorOr<User>> RegisterUserAsync(UserIn userIn, CancellationToken cancellation)
    {
        var validationResult = await _userValidator.ValidateAsync(userIn, cancellation);
        if (!validationResult.IsValid)
            return validationResult.Errors.ToErrorList();

        // make sure there are no conflicts
        var conflictErrors = new List<Error>();
        if (await _userRepository.GetByUsernameAsync(userIn.Username, cancellation) is not null)
            conflictErrors.Add(UserErrors.UsernameTaken);
        if (await _userRepository.GetByEmailAsync(userIn.Email, cancellation) is not null)
            conflictErrors.Add(UserErrors.EmailTaken);

        if (conflictErrors.Count != 0) return conflictErrors;

        // create the user
        var salt = _passwordHasher.GenerateSalt();
        var hash = await _passwordHasher.HashPasswordAsync(userIn.Password, salt);

        var dbUser = new User()
        {
            Username = userIn.Username,
            Email = userIn.Email,
            PasswordHash = hash,
            PasswordSalt = salt,
        };
        await _userRepository.AddUserAsync(dbUser, cancellation);

        return dbUser;
    }

    /// <summary>
    /// Log a user in if the username/email and passwords are correct
    /// </summary>
    public async Task<ErrorOr<Tokens>> LoginUserAsync(UserLogin userAuth, CancellationToken cancellation)
    {
        var dbUser = await _userRepository.GetByEmailAsync(userAuth.UsernameOrEmail, cancellation)
            ?? await _userRepository.GetByUsernameAsync(userAuth.UsernameOrEmail, cancellation);
        if (dbUser is null) return UserErrors.UserNotFound;

        var isPasswordCorrect = await _passwordHasher.VerifyPassword(
            userAuth.Password,
            dbUser.PasswordHash,
            dbUser.PasswordSalt);
        if (!isPasswordCorrect) return UserErrors.BadCredentials;

        return new Tokens()
        {
            AccessToken = _tokenProvider.GenerateAccessToken(dbUser),
            RefreshToken = _tokenProvider.GenerateRefreshToken(dbUser),
        };
    }
}
