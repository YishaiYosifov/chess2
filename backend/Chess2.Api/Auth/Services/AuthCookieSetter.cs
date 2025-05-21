using Chess2.Api.Auth.Controllers;
using Chess2.Api.Shared.DTOs;
using Microsoft.Extensions.Options;

namespace Chess2.Api.Auth.Services;

public interface IAuthCookieSetter
{
    void SetAuthCookies(string accessToken, string refreshToken, HttpContext context);
    void RemoveAuthCookies(HttpContext context);
}

public class AuthCookieSetter(
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

    public void SetAuthCookies(string accessToken, string refreshToken, HttpContext context)
    {
        SetCookie(
            context,
            _jwtSettings.AccessTokenCookieName,
            accessToken,
            maxAge: _jwtSettings.AccessMaxAge
        );
        SetCookie(
            context,
            _jwtSettings.RefreshTokenCookieName,
            refreshToken,
            maxAge: _jwtSettings.RefreshMaxAge,
            path: GetRefreshPath(context)
        );

        SetCookie(
            context,
            _jwtSettings.IsAuthedTokenCookieName,
            "true",
            maxAge: _jwtSettings.RefreshMaxAge,
            httpOnly: false
        );
    }

    public void RemoveAuthCookies(HttpContext context)
    {
        DeleteCookie(_jwtSettings.AccessTokenCookieName, context);
        DeleteCookie(_jwtSettings.RefreshTokenCookieName, context, path: GetRefreshPath(context));

        DeleteCookie(_jwtSettings.IsAuthedTokenCookieName, context);
    }

    private void SetCookie(
        HttpContext context,
        string name,
        string value,
        TimeSpan maxAge,
        bool httpOnly = true,
        string? path = "/"
    )
    {
        context.Response.Cookies.Append(
            name,
            value,
            new()
            {
                MaxAge = maxAge,
                HttpOnly = httpOnly,
                IsEssential = true,
                Secure = true,
                SameSite = _sameSiteMode,
                Path = path,
            }
        );
    }

    private void DeleteCookie(string name, HttpContext context, string? path = "/")
    {
        context.Response.Cookies.Delete(
            name,
            new()
            {
                Secure = true,
                SameSite = _sameSiteMode,
                Path = path,
            }
        );
    }

    private string? GetRefreshPath(HttpContext context) =>
        _linkGenerator.GetPathByName(context, nameof(AuthController.Refresh));
}
