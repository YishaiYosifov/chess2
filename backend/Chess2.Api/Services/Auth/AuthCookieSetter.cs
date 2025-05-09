using Chess2.Api.Models;
using Microsoft.Extensions.Options;

namespace Chess2.Api.Services.Auth;

public interface IAuthCookieSetter
{
    void SetAccessCookie(string accessToken, HttpContext context);
    void SetRefreshCookie(string refreshToken, HttpContext context);
}

public class AuthCookieSetter(IOptions<AppSettings> settings, IWebHostEnvironment hostEnvironment)
    : IAuthCookieSetter
{
    private readonly SameSiteMode _sameSiteMode = hostEnvironment.IsDevelopment()
        ? SameSiteMode.None
        : SameSiteMode.Strict;
    private readonly JwtSettings _jwtSettings = settings.Value.Jwt;

    public void SetAccessCookie(string accessToken, HttpContext context)
    {
        context.Response.Cookies.Append(
            _jwtSettings.AccessTokenCookieName,
            accessToken,
            new()
            {
                MaxAge = TimeSpan.FromSeconds(_jwtSettings.AccessExpiresInSeconds),
                HttpOnly = true,
                IsEssential = true,
                Secure = true,
                SameSite = _sameSiteMode,
            }
        );
    }

    public void SetRefreshCookie(string refreshToken, HttpContext context)
    {
        context.Response.Cookies.Append(
            _jwtSettings.RefreshTokenCookieName,
            refreshToken,
            new()
            {
                MaxAge = TimeSpan.FromDays(_jwtSettings.RefreshExpiresInDays),
                HttpOnly = true,
                IsEssential = true,
                Secure = true,
                SameSite = _sameSiteMode,
            }
        );
    }
}
