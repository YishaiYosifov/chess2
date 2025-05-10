using Chess2.Api.Errors;
using Chess2.Api.Extensions;
using Chess2.Api.Models;
using Chess2.Api.Models.DTOs;
using Chess2.Api.Models.Entities;
using ErrorOr;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Chess2.Api.Services.Auth;

public interface IAuthService
{
    Task<ErrorOr<AuthedUser>> GetLoggedInUserAsync(ClaimsPrincipal? userClaims);
    Task<ErrorOr<AuthedUser>> SignupAsync(string username, string email);
    Task<ErrorOr<Tokens>> SigninAsync(AuthedUser user, UserLoginInfo loginInfo);
    Task<ErrorOr<(Tokens newTokens, AuthedUser user)>> RefreshTokenAsync(
        ClaimsPrincipal? userClaims
    );
    void Logout(HttpContext context);
}

public class AuthService(
    IOptions<AppSettings> settings,
    ITokenProvider tokenProvider,
    ILogger<AuthService> logger,
    UserManager<AuthedUser> userManager
) : IAuthService
{
    private readonly AppSettings _settings = settings.Value;
    private readonly ITokenProvider _tokenProvider = tokenProvider;
    private readonly UserManager<AuthedUser> _userManager = userManager;
    private readonly ILogger<AuthService> _logger = logger;

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
    /// Create a new user
    /// </summary>
    /// <param name="signupRequest">The user DTO received from the client</param>
    public async Task<ErrorOr<AuthedUser>> SignupAsync(string username, string email)
    {
        var dbUser = new AuthedUser() { UserName = username, Email = email };

        var createdUserResult = await _userManager.CreateAsync(dbUser);
        if (!createdUserResult.Succeeded)
        {
            _logger.LogWarning(
                "Failed to create user {Username} with errors: {Errors}",
                username,
                createdUserResult.Errors
            );
            return createdUserResult.Errors.ToErrorList();
        }

        return dbUser;
    }

    /// <summary>
    /// Log a user in if the username/email and passwords are correct
    /// </summary>
    public async Task<ErrorOr<Tokens>> SigninAsync(AuthedUser user, UserLoginInfo loginInfo)
    {
        var loginResult = await HandleLoginMethods(user, loginInfo);
        if (loginResult.IsError)
            return loginResult.Errors;

        var accessTokenExpiresTimestamp = DateTimeOffset
            .UtcNow.AddSeconds(_settings.Jwt.AccessExpiresInSeconds)
            .ToUnixTimeSeconds();
        var tokens = new Tokens()
        {
            AccessToken = _tokenProvider.GenerateAccessToken(user),
            AccessTokenExpiresTimestamp = accessTokenExpiresTimestamp,
            RefreshToken = _tokenProvider.GenerateRefreshToken(user),
        };
        return tokens;
    }

    private async Task<ErrorOr<Success>> HandleLoginMethods(
        AuthedUser user,
        UserLoginInfo loginInfo
    )
    {
        var existingLogin = await _userManager.FindByLoginAsync(
            loginInfo.LoginProvider,
            loginInfo.ProviderKey
        );
        if (existingLogin is not null)
            return existingLogin.Id == user.Id ? Result.Success : AuthErrors.OAuthLoginConflict;

        var loginResult = await _userManager.AddLoginAsync(user, loginInfo);
        if (!loginResult.Succeeded)
            return loginResult.Errors.ToErrorList();
        return Result.Success;
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
