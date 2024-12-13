using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Chess2.Api.Errors;
using Chess2.Api.Extensions;
using Chess2.Api.Models;
using Chess2.Api.Models.DTOs;
using Chess2.Api.Models.Entities;
using Chess2.Api.Repositories;
using ErrorOr;
using FluentValidation;
using Microsoft.Extensions.Options;

namespace Chess2.Api.Services;

public interface IAuthService
{
    Task<ErrorOr<AuthedUser>> GetLoggedInUserAsync(
        HttpContext context,
        CancellationToken cancellation = default
    );

    Task<ErrorOr<AuthedUser>> SignupUserAsync(
        UserIn userIn,
        CancellationToken cancellation = default
    );

    Task<ErrorOr<Tokens>> LoginUserAsync(
        UserLogin userAuth,
        CancellationToken cancellation = default
    );

    void SetAccessCookie(string accessToken, HttpContext context);

    void SetRefreshCookie(string refreshToken, HttpContext context);

    Task<ErrorOr<string>> RefreshTokenAsync(
        HttpContext context,
        CancellationToken cancellation = default
    );
}

public class AuthService(
    IWebHostEnvironment hostEnvironment,
    IValidator<UserIn> userValidator,
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    IOptions<AppSettings> settings,
    ITokenProvider tokenProvider,
    ILogger<AuthService> logger
) : IAuthService
{
    private readonly IWebHostEnvironment _hostEnvironment = hostEnvironment;
    private readonly IValidator<UserIn> _userValidator = userValidator;
    private readonly IUserRepository _userRepository = userRepository;
    private readonly IPasswordHasher _passwordHasher = passwordHasher;
    private readonly ITokenProvider _tokenProvider = tokenProvider;
    private readonly JwtSettings _jwtSettings = settings.Value.Jwt;
    private readonly ILogger<AuthService> _logger = logger;

    /// <summary>
    /// Get the user that is logged in to the http context
    /// </summary>
    public async Task<ErrorOr<AuthedUser>> GetLoggedInUserAsync(
        HttpContext context,
        CancellationToken cancellation = default
    )
    {
        var userIdClaimResult = context.User.GetClaim(ClaimTypes.NameIdentifier);
        if (userIdClaimResult.IsError)
        {
            _logger.LogWarning(
                "A user tried to access an authorized endpoint but the user id claim could not be found"
            );
            return userIdClaimResult.Errors;
        }
        var userId = Convert.ToInt32(userIdClaimResult.Value.Value);

        var user = await _userRepository.GetByUserIdAsync(userId, cancellation);
        if (user is null)
        {
            _logger.LogWarning(
                "A user tried to access an authorized enpoint with id {UserId} but the user could not be found",
                userId
            );
            return Error.Unauthorized();
        }

        return user;
    }

    /// <summary>
    /// Register a new user
    /// </summary>
    /// <param name="userIn">The user DTO received from the client</param>
    public async Task<ErrorOr<AuthedUser>> SignupUserAsync(
        UserIn userIn,
        CancellationToken cancellation = default
    )
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

        if (conflictErrors.Count != 0)
            return conflictErrors;

        // create the user
        var salt = _passwordHasher.GenerateSalt();
        var hash = await _passwordHasher.HashPasswordAsync(userIn.Password, salt);

        var dbUser = new AuthedUser()
        {
            Username = userIn.Username,
            Email = userIn.Email,
            CountryCode = userIn.CountryCode,
            PasswordHash = hash,
            PasswordSalt = salt,
        };
        await _userRepository.AddUserAsync(dbUser, cancellation);

        return dbUser;
    }

    /// <summary>
    /// Log a user in if the username/email and passwords are correct
    /// </summary>
    public async Task<ErrorOr<Tokens>> LoginUserAsync(
        UserLogin userAuth,
        CancellationToken cancellation = default
    )
    {
        var dbUser =
            await _userRepository.GetByEmailAsync(userAuth.UsernameOrEmail, cancellation)
            ?? await _userRepository.GetByUsernameAsync(userAuth.UsernameOrEmail, cancellation);
        if (dbUser is null)
            return UserErrors.BadCredentials;

        var isPasswordCorrect = await _passwordHasher.VerifyPassword(
            userAuth.Password,
            dbUser.PasswordHash,
            dbUser.PasswordSalt
        );
        if (!isPasswordCorrect)
            return UserErrors.BadCredentials;

        return new Tokens()
        {
            AccessToken = _tokenProvider.GenerateAccessToken(dbUser),
            RefreshToken = _tokenProvider.GenerateRefreshToken(dbUser),
        };
    }

    public void SetAccessCookie(string accessToken, HttpContext context)
    {
        var accessTokenExpires = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessExpiresInMinute);
        context.Response.Cookies.Append(
            _jwtSettings.AccessTokenCookieName,
            accessToken,
            new()
            {
                Expires = accessTokenExpires,
                HttpOnly = true,
                IsEssential = true,
                Secure = !_hostEnvironment.IsDevelopment(),
                SameSite = SameSiteMode.Strict,
            }
        );
    }

    public void SetRefreshCookie(string refreshToken, HttpContext context)
    {
        var refreshTokenExpires = DateTime.UtcNow.AddDays(_jwtSettings.RefreshExpiresInDays);
        context.Response.Cookies.Append(
            _jwtSettings.RefreshTokenCookieName,
            refreshToken,
            new()
            {
                Expires = refreshTokenExpires,
                HttpOnly = true,
                IsEssential = true,
                Secure = !_hostEnvironment.IsDevelopment(),
                SameSite = SameSiteMode.Strict,
            }
        );
    }

    /// <summary>
    /// Validate the refresh token is valid and
    /// create an access token from it
    /// </summary>
    public async Task<ErrorOr<string>> RefreshTokenAsync(
        HttpContext context,
        CancellationToken cancellation = default
    )
    {
        var tokenCreationTimeClaimResult = context.User.GetClaim(JwtRegisteredClaimNames.Iat);
        if (tokenCreationTimeClaimResult.IsError)
            return tokenCreationTimeClaimResult.Errors;

        var userResult = await GetLoggedInUserAsync(context, cancellation);
        if (userResult.IsError)
            return userResult.Errors;
        var user = userResult.Value;

        // make sure the password hasn't changed since the refresh token was created
        // this way we can invalidate leaked tokens
        var passwordChangedTimestamp = (
            (DateTimeOffset)user.PasswordLastChanged
        ).ToUnixTimeSeconds();
        var tokenCreationTimestamp = Convert.ToInt64(tokenCreationTimeClaimResult.Value.Value);
        if (tokenCreationTimestamp < passwordChangedTimestamp)
            return Error.Unauthorized();

        return _tokenProvider.GenerateAccessToken(user);
    }
}
