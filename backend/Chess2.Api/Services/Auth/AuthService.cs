using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Chess2.Api.Errors;
using Chess2.Api.Extensions;
using Chess2.Api.Models;
using Chess2.Api.Models.DTOs;
using Chess2.Api.Models.Entities;
using ErrorOr;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Chess2.Api.Services.Auth;

public interface IAuthService
{
    Task<ErrorOr<AuthedUser>> GetLoggedInUserAsync(ClaimsPrincipal? userClaims);
    Task<ErrorOr<AuthedUser>> SignupAsync(SignupRequest userIn);
    Task<ErrorOr<(Tokens tokens, AuthedUser user)>> SigninAsync(
        string usernameOrEmail,
        string password
    );
    Task<ErrorOr<(Tokens newTokens, AuthedUser user)>> RefreshTokenAsync(
        ClaimsPrincipal? userClaims
    );
    void Logout(HttpContext context);
}

public class AuthService(
    IOptions<AppSettings> settings,
    ITokenProvider tokenProvider,
    ILogger<AuthService> logger,
    UserManager<AuthedUser> userManager,
    SignInManager<AuthedUser> signinManager,
    IAuthCookieSetter authCookieSetter
) : IAuthService
{
    private readonly AppSettings _settings = settings.Value;
    private readonly ITokenProvider _tokenProvider = tokenProvider;
    private readonly UserManager<AuthedUser> _userManager = userManager;
    private readonly SignInManager<AuthedUser> _signinManager = signinManager;
    private readonly ILogger<AuthService> _logger = logger;
    private readonly IAuthCookieSetter _authCookieSetter = authCookieSetter;

    /// <summary>
    /// Get the user that is logged in to the http context
    /// </summary>
    public async Task<ErrorOr<AuthedUser>> GetLoggedInUserAsync(ClaimsPrincipal? userClaims)
    {
        var userIdClaimResult = userClaims.GetClaim(ClaimTypes.NameIdentifier);
        if (userIdClaimResult.IsError)
        {
            _logger.LogWarning(
                "A user tried to access an authorized endpoint but the user id claim could not be found"
            );
            return userIdClaimResult.Errors;
        }
        var userId = userIdClaimResult.Value.Value;

        var user = await _userManager.FindByIdAsync(userId);
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
    /// <param name="signupRequest">The user DTO received from the client</param>
    public async Task<ErrorOr<AuthedUser>> SignupAsync(SignupRequest signupRequest)
    {
        var dbUser = new AuthedUser()
        {
            UserName = signupRequest.UserName,
            Email = signupRequest.Email,
            CountryCode = signupRequest.CountryCode,
        };

        // make sure there are no conflicts
        var conflictErrors = new List<Error>();
        if (await _userManager.FindByNameAsync(signupRequest.UserName) is not null)
            conflictErrors.Add(UserErrors.UsernameTaken);
        if (await _userManager.FindByEmailAsync(signupRequest.Email) is not null)
            conflictErrors.Add(UserErrors.EmailTaken);

        if (conflictErrors.Count != 0)
            return conflictErrors;

        var createdUserResult = await _userManager.CreateAsync(dbUser, signupRequest.Password);
        if (!createdUserResult.Succeeded)
        {
            _logger.LogWarning(
                "Failed to create user {Username} with errors: {Errors}",
                signupRequest.UserName,
                createdUserResult.Errors
            );
            return createdUserResult.Errors.ToErrorList();
        }

        return dbUser;
    }

    /// <summary>
    /// Log a user in if the username/email and passwords are correct
    /// </summary>
    public async Task<ErrorOr<(Tokens tokens, AuthedUser user)>> SigninAsync(
        string usernameOrEmail,
        string password
    )
    {
        var user =
            await _userManager.FindByEmailAsync(usernameOrEmail)
            ?? await _userManager.FindByNameAsync(usernameOrEmail);
        if (user is null)
            return UserErrors.BadCredentials;

        var signinResult = await _signinManager.CheckPasswordSignInAsync(
            user,
            password,
            lockoutOnFailure: false
        );
        if (!signinResult.Succeeded)
            return UserErrors.BadCredentials;

        var accessTokenExpiresTimestamp = DateTimeOffset
            .UtcNow.AddSeconds(_settings.Jwt.AccessExpiresInSeconds)
            .ToUnixTimeSeconds();
        var tokens = new Tokens()
        {
            AccessToken = _tokenProvider.GenerateAccessToken(user),
            AccessTokenExpiresTimestamp = accessTokenExpiresTimestamp,
            RefreshToken = _tokenProvider.GenerateRefreshToken(user),
        };
        return (tokens, user);
    }

    /// <summary>
    /// Validate the refresh token is valid and
    /// create an access token from it
    /// </summary>
    public async Task<ErrorOr<(Tokens newTokens, AuthedUser user)>> RefreshTokenAsync(
        ClaimsPrincipal? userClaims
    )
    {
        var tokenCreationTimeClaimResult = userClaims.GetClaim(JwtRegisteredClaimNames.Iat);
        if (tokenCreationTimeClaimResult.IsError)
            return tokenCreationTimeClaimResult.Errors;

        var userResult = await GetLoggedInUserAsync(userClaims);
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

        var accessTokenExpiresTimestamp = DateTimeOffset
            .UtcNow.AddSeconds(_settings.Jwt.AccessExpiresInSeconds)
            .ToUnixTimeSeconds();
        var newAccessToken = _tokenProvider.GenerateAccessToken(user);
        var newRefreshToken = ""; // TODO!!!!
        var newTokens = new Tokens()
        {
            AccessToken = newAccessToken,
            AccessTokenExpiresTimestamp = accessTokenExpiresTimestamp,
            RefreshToken = newRefreshToken,
        };

        return (newTokens, user);
    }

    public void Logout(HttpContext context)
    {
        foreach (var cookie in context.Request.Cookies)
        {
            context.Response.Cookies.Delete(cookie.Key);
        }
    }
}
