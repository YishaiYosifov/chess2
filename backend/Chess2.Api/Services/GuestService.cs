using System.Security.Claims;
using Chess2.Api.Extensions;
using Chess2.Api.Models;
using ErrorOr;
using Microsoft.Extensions.Options;

namespace Chess2.Api.Services;

public interface IGuestService
{
    string CreateGuestUser();
    void SetGuestCookie(string guestToken, HttpContext context);

    ErrorOr<User> GetAnonUserAsync(ClaimsPrincipal? userClaims);
}

public class GuestService(
    ILogger<GuestService> logger,
    ITokenProvider tokenProvider,
    IOptions<AppSettings> settings,
    IWebHostEnvironment hostEnvironment
) : IGuestService
{
    private readonly IWebHostEnvironment _hostEnvironment = hostEnvironment;
    private readonly ITokenProvider _tokenProvider = tokenProvider;
    private readonly JwtSettings _jwtSettings = settings.Value.Jwt;
    private readonly ILogger<GuestService> _logger = logger;

    /// <summary>
    /// Create a stateless guest user id and its access token
    /// </summary>
    /// <returns>The guest access token</returns>
    public string CreateGuestUser()
    {
        var id = GenerateGuestId();
        var accessToken = _tokenProvider.GenerateGuestToken(id);
        return accessToken;
    }

    public void SetGuestCookie(string guestToken, HttpContext context)
    {
        // session cookie
        context.Response.Cookies.Append(
            _jwtSettings.AccessTokenCookieName,
            guestToken,
            new()
            {
                HttpOnly = true,
                IsEssential = true,
                Secure = !_hostEnvironment.IsDevelopment(),
                SameSite = SameSiteMode.Strict,
            }
        );
    }

    public ErrorOr<User> GetAnonUserAsync(ClaimsPrincipal? userClaims)
    {
        var userIdClaimResult = userClaims.GetClaim(ClaimTypes.NameIdentifier);
        if (userIdClaimResult.IsError)
        {
            _logger.LogWarning(
                "A user tried to access a anonymous authorized endpoint "
                    + "but the user id claim could not be found"
            );
            return userIdClaimResult.Errors;
        }
        var userId = userIdClaimResult.Value.Value;

        var anonymousClaimResult = userClaims.GetClaim(ClaimTypes.Anonymous);
        var isGuest = !anonymousClaimResult.IsError && anonymousClaimResult.Value.Value == "1";
        _logger.LogInformation("Got user id {UserId}, is guest: {IsGuest}", userId, isGuest);

        return new User() { UserId = userId, IsGuest = isGuest };
    }

    private static string GenerateGuestId()
    {
        return $"{Guid.NewGuid()}-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}";
    }
}
