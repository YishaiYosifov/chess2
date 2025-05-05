using Chess2.Api.Controllers;
using Chess2.Api.Models;
using Microsoft.Extensions.Options;

namespace Chess2.Api.Services.Auth;

public interface IAuthCookieSetter
{
    void SetAccessCookie(string accessToken, HttpContext context);
    void SetRefreshCookie(string refreshToken, HttpContext context);
}

public class AuthCookieSetter(
    IOptions<AppSettings> settings,
    IWebHostEnvironment hostEnvironment,
    LinkGenerator linkGenerator
) : IAuthCookieSetter
{
    private readonly JwtSettings _jwtSettings = settings.Value.Jwt;
    private readonly IWebHostEnvironment _hostEnvironment = hostEnvironment;
    private readonly LinkGenerator _linkGenerator = linkGenerator;

    public void SetAccessCookie(string accessToken, HttpContext context)
    {
        var accessTokenExpires = DateTime.UtcNow.AddSeconds(_jwtSettings.AccessExpiresInSeconds);
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
}
