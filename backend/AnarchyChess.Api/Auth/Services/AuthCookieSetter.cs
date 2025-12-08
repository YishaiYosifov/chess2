using AnarchyChess.Api.Auth.Controllers;
using AnarchyChess.Api.Shared.Models;
using ErrorOr;
using Microsoft.Extensions.Options;

namespace AnarchyChess.Api.Auth.Services;

public interface IAuthCookieSetter
{
    void SetAuthCookies(string accessToken, string refreshToken, HttpContext context);
    void RemoveAuthCookies(HttpContext context);
    void SetGuestCookie(string accessToken, HttpContext context);
    void SetAuthFailureCookie(Error reason, HttpContext context);
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
    private readonly AuthSettings _settings = settings.Value.Auth;
    private readonly LinkGenerator _linkGenerator = linkGenerator;

    public void SetAuthCookies(string accessToken, string refreshToken, HttpContext context)
    {
        SetCookie(
            context,
            _settings.AccessTokenCookieName,
            accessToken,
            maxAge: _settings.AccessMaxAge
        );
        SetCookie(
            context,
            _settings.RefreshTokenCookieName,
            refreshToken,
            maxAge: _settings.RefreshMaxAge,
            path: GetRefreshPath(context)
        );

        SetCookie(
            context,
            _settings.IsLoggedInCookieName,
            "true",
            maxAge: _settings.RefreshMaxAge,
            httpOnly: false
        );
    }

    public void RemoveAuthCookies(HttpContext context)
    {
        DeleteCookie(_settings.AccessTokenCookieName, context);
        DeleteCookie(_settings.RefreshTokenCookieName, context, path: GetRefreshPath(context));

        DeleteCookie(_settings.IsLoggedInCookieName, context);
    }

    public void SetGuestCookie(string accessToken, HttpContext context) =>
        SetCookie(context, _settings.AccessTokenCookieName, accessToken);

    public void SetAuthFailureCookie(Error reason, HttpContext context) =>
        SetCookie(context, _settings.AuthFailureCookieName, reason.Code, httpOnly: false);

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
                Domain = _settings.CookieDomain,
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
                Domain = _settings.CookieDomain,
                Path = path,
            }
        );
    }

    private string? GetRefreshPath(HttpContext context) =>
        _linkGenerator.GetPathByName(context, nameof(AuthController.Refresh));
}
