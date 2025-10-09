using Chess2.Api.Profile.Models;
using Chess2.Api.Shared.Models;
using Microsoft.Extensions.Options;
using OpenIddict.Abstractions;
using System.Security.Claims;

namespace Chess2.Api.Auth.Services;

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
        var id = UserId.Guest();
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
                Secure = true,
                SameSite = SameSiteMode.None,
            }
        );
    }

    /// <summary>
    /// Find whether the user claims indicate the user is a guest or not
    /// </summary>
    public bool IsGuest(ClaimsPrincipal? userClaims)
    {
        if (userClaims is null)
            return false;

        var isAnnonymous = userClaims.GetClaim(ClaimTypes.Anonymous);
        var isGuest = isAnnonymous is not null && isAnnonymous == "1";
        return isGuest;
    }
}
