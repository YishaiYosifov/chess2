using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Chess2.Api.Errors;
using Chess2.Api.Extensions;
using Chess2.Api.Models.DTOs;
using Chess2.Api.Models.Entities;
using ErrorOr;
using Microsoft.AspNetCore.Identity;
using OpenIddict.Abstractions;

namespace Chess2.Api.Services.Auth;

public interface IAuthService
{
    Task<ErrorOr<AuthedUser>> GetLoggedInUserAsync(ClaimsPrincipal? userClaims);
    Task<Tokens> GenerateAuthTokensAsync(AuthedUser user, CancellationToken token = default);
    Task<ErrorOr<AuthedUser>> SignupAsync(
        string username,
        string? email = null,
        string? countryCode = null
    );
    Task<ErrorOr<Tokens>> RefreshTokenAsync(
        ClaimsPrincipal? claimsPrincipal,
        CancellationToken token = default
    );
}

public class AuthService(
    ILogger<AuthService> logger,
    ITokenProvider tokenProvider,
    UserManager<AuthedUser> userManager,
    IRefreshTokenService refreshTokenService,
    IUnitOfWork unitOfWork
) : IAuthService
{
    private readonly ILogger<AuthService> _logger = logger;
    private readonly ITokenProvider _tokenProvider = tokenProvider;
    private readonly UserManager<AuthedUser> _userManager = userManager;
    private readonly IRefreshTokenService _refreshTokenService = refreshTokenService;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    /// <summary>
    /// Get the user that is logged in to the http context
    /// </summary>
    public async Task<ErrorOr<AuthedUser>> GetLoggedInUserAsync(ClaimsPrincipal? claimsPrincipal)
    {
        if (claimsPrincipal is null)
        {
            _logger.LogWarning(
                "A user tried to access an authorized endpoint but the claims principal was null"
            );
            return Error.Unauthorized();
        }

        var user = await _userManager.GetUserAsync(claimsPrincipal);
        if (user is null)
        {
            _logger.LogWarning(
                "A user tried to access an authorized endpoint but the user could not be found"
            );
            return Error.Unauthorized();
        }

        return user;
    }

    /// <summary>
    /// Create a new user
    /// </summary>
    /// <param name="signupRequest">The user DTO received from the client</param>
    public async Task<ErrorOr<AuthedUser>> SignupAsync(
        string username,
        string? email = null,
        string? countryCode = null
    )
    {
        var dbUser = new AuthedUser()
        {
            UserName = username,
            Email = email,
            CountryCode = countryCode,
        };

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

    public async Task<Tokens> GenerateAuthTokensAsync(
        AuthedUser user,
        CancellationToken token = default
    )
    {
        var refreshTokenRecord = await _refreshTokenService.CreateRefreshTokenAsync(user, token);

        var accessToken = _tokenProvider.GenerateAccessToken(user);
        var refreshToken = _tokenProvider.GenerateRefreshToken(user, refreshTokenRecord.Jti);

        var tokens = new Tokens(accessToken, refreshToken);
        return tokens;
    }

    public async Task<ErrorOr<Tokens>> RefreshTokenAsync(
        ClaimsPrincipal? claimsPrincipal,
        CancellationToken token = default
    )
    {
        if (claimsPrincipal is null)
            return Error.Unauthorized();
        var userResult = await GetLoggedInUserAsync(claimsPrincipal);
        if (userResult.IsError)
            return userResult.Errors;
        var user = userResult.Value;

        var jti = claimsPrincipal.GetClaim(JwtRegisteredClaimNames.Jti);
        if (jti is null)
            return AuthErrors.TokenInvalid;

        var refreshResult = await _refreshTokenService.ConsumeRefreshTokenAsync(user, jti, token);
        if (refreshResult.IsError)
            return refreshResult.Errors;

        var tokens = await GenerateAuthTokensAsync(user, token);
        await _unitOfWork.CompleteAsync(token);
        return tokens;
    }
}
