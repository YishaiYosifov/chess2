using Chess2.Api.Auth.DTOs;
using Chess2.Api.Auth.Errors;
using Chess2.Api.Infrastructure.Extensions;
using Chess2.Api.Profile.Entities;
using Chess2.Api.Profile.Models;
using Chess2.Api.Shared.Services;
using ErrorOr;
using Microsoft.AspNetCore.Identity;
using OpenIddict.Abstractions;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Chess2.Api.Auth.Services;

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
    ErrorOr<UserId> GetUserId(ClaimsPrincipal? claimsPrincipal);
    Task<ErrorOr<T>> MatchAuthTypeAsync<T>(
        ClaimsPrincipal? claimsPrincipal,
        Func<AuthedUser, Task<T>> whenAuthed,
        Func<UserId, Task<T>> whenGuest
    );
}

public class AuthService(
    ILogger<AuthService> logger,
    ITokenProvider tokenProvider,
    UserManager<AuthedUser> userManager,
    IRefreshTokenService refreshTokenService,
    IGuestService guestService,
    IUnitOfWork unitOfWork
) : IAuthService
{
    private readonly ILogger<AuthService> _logger = logger;
    private readonly ITokenProvider _tokenProvider = tokenProvider;
    private readonly UserManager<AuthedUser> _userManager = userManager;
    private readonly IRefreshTokenService _refreshTokenService = refreshTokenService;
    private readonly IGuestService _guestService = guestService;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public ErrorOr<UserId> GetUserId(ClaimsPrincipal? claimsPrincipal)
    {
        var userId = claimsPrincipal?.GetClaim(ClaimTypes.NameIdentifier);
        return userId is null ? Error.Unauthorized() : (UserId)userId;
    }

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

    public async Task<ErrorOr<T>> MatchAuthTypeAsync<T>(
        ClaimsPrincipal? claimsPrincipal,
        Func<AuthedUser, Task<T>> whenAuthed,
        Func<UserId, Task<T>> whenGuest
    )
    {
        var userId = GetUserId(claimsPrincipal);
        if (userId.IsError)
            return userId.Errors;

        if (_guestService.IsGuest(claimsPrincipal))
            return await whenGuest(userId.Value);

        var authedUser = await GetLoggedInUserAsync(claimsPrincipal);
        if (authedUser.IsError)
            return authedUser.Errors;

        return await whenAuthed(authedUser.Value);
    }

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
            About = "",
            CountryCode = countryCode ?? "XX",
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
