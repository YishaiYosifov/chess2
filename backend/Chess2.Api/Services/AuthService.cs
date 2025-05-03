using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Chess2.Api.Controllers;
using Chess2.Api.Errors;
using Chess2.Api.Extensions;
using Chess2.Api.Models;
using Chess2.Api.Models.DTOs;
using Chess2.Api.Models.Entities;
using ErrorOr;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Chess2.Api.Services;

public interface IAuthService
{
    Task<ErrorOr<AuthedUser>> GetLoggedInUserAsync(ClaimsPrincipal? userClaims);

    Task<ErrorOr<AuthedUser>> SignupAsync(SignupRequest userIn);

    Task<ErrorOr<Tokens>> SigninAsync(string usernameOrEmail, string password);

    void SetAccessCookie(string accessToken, HttpContext context);

    void SetRefreshCookie(string refreshToken, HttpContext context);

    Task<ErrorOr<string>> RefreshTokenAsync(
        ClaimsPrincipal? userClaims,
        CancellationToken cancellation = default
    );
}

public class AuthService(
    IWebHostEnvironment hostEnvironment,
    IOptions<AppSettings> settings,
    ITokenProvider tokenProvider,
    ILogger<AuthService> logger,
    UserManager<AuthedUser> userManager,
    SignInManager<AuthedUser> signinManager,
    LinkGenerator linkGenerator
) : IAuthService
{
    private readonly IWebHostEnvironment _hostEnvironment = hostEnvironment;
    private readonly ITokenProvider _tokenProvider = tokenProvider;
    private readonly UserManager<AuthedUser> _userManager = userManager;
    private readonly SignInManager<AuthedUser> _signinManager = signinManager;
    private readonly JwtSettings _jwtSettings = settings.Value.Jwt;
    private readonly ILogger<AuthService> _logger = logger;
    private readonly LinkGenerator _linkGenerator = linkGenerator;

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
    public async Task<ErrorOr<Tokens>> SigninAsync(string usernameOrEmail, string password)
    {
        var user =
            (await _userManager.FindByEmailAsync(usernameOrEmail))
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

        return new Tokens()
        {
            AccessToken = _tokenProvider.GenerateAccessToken(user),
            RefreshToken = _tokenProvider.GenerateRefreshToken(user),
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
                Secure = true,
                SameSite = _hostEnvironment.IsDevelopment()
                    ? SameSiteMode.None
                    : SameSiteMode.Strict,
            }
        );
    }

    public void SetRefreshCookie(string refreshToken, HttpContext context)
    {
        var refreshPath = _linkGenerator.GetPathByName(context, nameof(AuthController.Refresh));
        var refreshTokenExpires = DateTime.UtcNow.AddDays(_jwtSettings.RefreshExpiresInDays);
        context.Response.Cookies.Append(
            _jwtSettings.RefreshTokenCookieName,
            refreshToken,
            new()
            {
                Expires = refreshTokenExpires,
                HttpOnly = true,
                IsEssential = true,
                Secure = true,
                SameSite = _hostEnvironment.IsDevelopment()
                    ? SameSiteMode.None
                    : SameSiteMode.Strict,
                Path = refreshPath,
            }
        );
    }

    /// <summary>
    /// Validate the refresh token is valid and
    /// create an access token from it
    /// </summary>
    public async Task<ErrorOr<string>> RefreshTokenAsync(
        ClaimsPrincipal? userClaims,
        CancellationToken cancellation = default
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

        return _tokenProvider.GenerateAccessToken(user);
    }
}
