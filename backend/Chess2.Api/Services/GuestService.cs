using Chess2.Api.Models;
using Microsoft.Extensions.Options;

namespace Chess2.Api.Services;

public interface IGuestService
{
    string CreateGuestUser();
    void SetGuestCookie(string guestToken, HttpContext context);
}

public class GuestService(
    ITokenProvider tokenProvider,
    IOptions<AppSettings> settings,
    IWebHostEnvironment hostEnvironment) : IGuestService
{
    private readonly IWebHostEnvironment _hostEnvironment = hostEnvironment;
    private readonly ITokenProvider _tokenProvider = tokenProvider;
    private readonly JwtSettings _jwtSettings = settings.Value.Jwt;


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
            });
    }


    private string GenerateGuestId()
    {
        return $"{Guid.NewGuid()}-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}";
    }
}
