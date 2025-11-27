using AnarchyChess.Api.Auth.Controllers;
using AnarchyChess.Api.Shared.Models;
using Microsoft.Extensions.Options;

namespace AnarchyChess.Api.Auth.Services;

public interface IAuthCookieSetter
{
    void SetAuthCookies(string accessToken, string refreshToken, HttpContext context);
    void RemoveAuthCookies(HttpContext context);
    void SetGuestCookie(string accessToken, HttpContext context);
}

public class AuthCookieSetter(
    IOptions<AppSettings> settings,
    IWebHostEnvironment hostEnvironment,
    LinkGenerator linkGenerator
) : IAuthCookieSetter
{
    private readonly SameSiteMode _sameSiteMode = hostEnvironment.IsDevelopment()
        ? SameSiteMode.None
        : SameSiteMode.Lax;
    private readonly AuthSettings _jwtSettings = settings.Value.Auth;
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
            _jwtSettings.IsLoggedInCookieName,
            "true",
            maxAge: _jwtSettings.RefreshMaxAge,
            httpOnly: false
        );
    }

    public void RemoveAuthCookies(HttpContext context)
    {
        DeleteCookie(_jwtSettings.AccessTokenCookieName, context);
        DeleteCookie(_jwtSettings.RefreshTokenCookieName, context, path: GetRefreshPath(context));

        DeleteCookie(_jwtSettings.IsLoggedInCookieName, context);
    }

    public void SetGuestCookie(string accessToken, HttpContext context) =>
        SetCookie(context, _jwtSettings.AccessTokenCookieName, accessToken);

    private void SetCookie(
        HttpContext context,
        string name,
        string value,
        TimeSpan? maxAge = null,
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
