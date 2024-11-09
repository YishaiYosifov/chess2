using Chess2.Api.Errors;
using Chess2.Api.Extensions;
using Chess2.Api.Models;
using Chess2.Api.Models.DTOs;
using Chess2.Api.Models.Entities;
using Chess2.Api.Repositories;
using ErrorOr;
using FluentValidation;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace Chess2.Api.Services;

public interface IAuthService
{
    Task<ErrorOr<User>> RegisterUserAsync(UserIn userIn, CancellationToken cancellation);
    Task<ErrorOr<Tokens>> LoginUserAsync(UserLogin userAuth, CancellationToken cancellation);
    Task<ErrorOr<User>> GetLoggedInUser(CancellationToken cancellation);

    string RefreshToken(string refreshToken);
    void SetTokenCookies(Tokens tokens, HttpContext context);
}

public class AuthService(
    IHttpContextAccessor httpContextAccessor,
    IWebHostEnvironment hostEnvironment,
    IValidator<UserIn> userValidator,
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    ITokenProvider tokenProvider,
    IOptions<AppSettings> settings) : IAuthService
{
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
    private readonly IWebHostEnvironment _hostEnvironment = hostEnvironment;
    private readonly IValidator<UserIn> _userValidator = userValidator;
    private readonly IUserRepository _userRepository = userRepository;
    private readonly IPasswordHasher _passwordHasher = passwordHasher;
    private readonly ITokenProvider _tokenProvider = tokenProvider;
    private readonly JwtSettings _jwtSettings = settings.Value.Jwt;

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

    public async Task<ErrorOr<User>> GetLoggedInUser(CancellationToken cancellation)
    {
        var userIdentities = _httpContextAccessor.HttpContext?.User;
        var userIdClaim = userIdentities?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
        if (userIdClaim is null
            || !int.TryParse(userIdClaim.Value, out var userId))
            return Error.Unauthorized();

        var user = await _userRepository.GetByUserIdAsync(userId, cancellation);
        if (user is null) return Error.Unauthorized();

        return user;
    }

    /// <summary>
    /// Add the access and refresh tokens to the response
    /// </summary>
    public void SetTokenCookies(Tokens tokens, HttpContext context)
    {
        var accessTokenExpires = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessExpiresInMinute);
        var refreshTokenExpires = DateTime.UtcNow.AddDays(_jwtSettings.RefreshExpiresInDays);

        context.Response.Cookies.Append(
            _jwtSettings.AccessTokenCookieName,
            tokens.AccessToken,
            new()
            {
                Expires = accessTokenExpires,
                HttpOnly = true,
                IsEssential = true,
                Secure = _hostEnvironment.IsDevelopment(),
                SameSite = SameSiteMode.Strict,
            });

        context.Response.Cookies.Append(
            _jwtSettings.RefreshTokenCookieName,
            tokens.RefreshToken,
            new()
            {
                Expires = refreshTokenExpires,
                HttpOnly = true,
                IsEssential = true,
                Secure = _hostEnvironment.IsDevelopment(),
                SameSite = SameSiteMode.Strict,
            });
    }

    public string RefreshToken(string refreshToken)
    {
        return "";
    }
}
