using Chess2.Api.Controllers;
using Chess2.Api.Models;
using Microsoft.Extensions.Options;

namespace Chess2.Api.Services.Auth;

public interface IAuthCookieSetter
{
    void SetAccessCookie(string accessToken, HttpContext context);
    void SetRefreshCookie(string refreshToken, HttpContext context);
    void SetIsAuthedCookie(HttpContext context);
    void RemoveAccessCookie(HttpContext context);
    void RemoveRefreshCookie(HttpContext context);
    void RemoveIsAuthedCookie(HttpContext context);
}

public class AuthCookieService(
    IOptions<AppSettings> settings,
    IWebHostEnvironment hostEnvironment,
    LinkGenerator linkGenerator
) : IAuthCookieSetter
{
    private readonly SameSiteMode _sameSiteMode = hostEnvironment.IsDevelopment()
        ? SameSiteMode.None
        : SameSiteMode.Strict;
    private readonly JwtSettings _jwtSettings = settings.Value.Jwt;
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
                SameSite = _sameSiteMode,
            }
        );
    }

    public void RemoveAccessCookie(HttpContext context) =>
        context.Response.Cookies.Delete(_jwtSettings.AccessTokenCookieName);

    public void SetRefreshCookie(string refreshToken, HttpContext context)
    {
        var refreshPath = _linkGenerator.GetPathByName(context, nameof(AuthController));
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
                SameSite = _sameSiteMode,
                Path = refreshPath,
            }
        );
    }

    public void RemoveRefreshCookie(HttpContext context)
    {
        var refreshPath = _linkGenerator.GetPathByName(context, nameof(AuthController));
        context.Response.Cookies.Delete(
            _jwtSettings.RefreshTokenCookieName,
            new() { Path = refreshPath }
        );
    }

    public void SetIsAuthedCookie(HttpContext context)
    {
        var refreshTokenExpires = DateTime.UtcNow.AddDays(_jwtSettings.RefreshExpiresInDays);
        context.Response.Cookies.Append(
            _jwtSettings.IsAuthedCookieName,
            "true",
            new()
            {
                Expires = refreshTokenExpires,
                IsEssential = true,
                Secure = true,
                SameSite = _sameSiteMode,
            }
        );
    }

    public void RemoveIsAuthedCookie(HttpContext context) =>
        context.Response.Cookies.Delete(_jwtSettings.IsAuthedCookieName);
}
