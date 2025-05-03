using System.Security.Claims;
using Chess2.Api.Extensions;
using Chess2.Api.Models;
using Microsoft.Extensions.Options;

namespace Chess2.Api.Services.Auth;

public interface IGuestService
{
    string CreateGuestUser();
    void SetGuestCookie(string guestToken, HttpContext context);

    bool IsGuest(ClaimsPrincipal? userClaims);
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
        _logger.LogInformation("Created guest user with id {Id}", id);
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

    /// <summary>
    /// Find whether the user claims indicate the user is a guest or not
    /// </summary>
    public bool IsGuest(ClaimsPrincipal? userClaims)
    {
        var anonymousClaimResult = userClaims.GetClaim(ClaimTypes.Anonymous);
        var isGuest = !anonymousClaimResult.IsError && anonymousClaimResult.Value.Value == "1";
        return isGuest;
    }

    private static string GenerateGuestId()
    {
        return $"{Guid.NewGuid()}-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}";
    }
}
