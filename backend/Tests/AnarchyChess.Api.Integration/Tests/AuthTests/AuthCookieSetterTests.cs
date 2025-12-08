using AnarchyChess.Api.Auth.Errors;
using AnarchyChess.Api.Auth.Services;
using AnarchyChess.Api.Shared.Models;
using AnarchyChess.Api.TestInfrastructure;
using AwesomeAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using NSubstitute;
using SameSiteMode = Microsoft.Net.Http.Headers.SameSiteMode;

namespace AnarchyChess.Api.Integration.Tests.AuthTests;

public class AuthCookieSetterTests : BaseIntegrationTest
{
    private readonly AuthSettings _settings;
    private readonly IOptions<AppSettings> _appSettingsOptions;

    private readonly IWebHostEnvironment _webHostEnvironmentMock =
        Substitute.For<IWebHostEnvironment>();
    private readonly LinkGenerator _linkGenerator;

    private SameSiteMode SameSiteMode =>
        _webHostEnvironmentMock.IsDevelopment() ? SameSiteMode.None : SameSiteMode.Lax;

    private const string _refreshPath = "/api/auth/refresh";

    public AuthCookieSetterTests(AnarchyChessWebApplicationFactory factory)
        : base(factory)
    {
        _appSettingsOptions = Scope.ServiceProvider.GetRequiredService<IOptions<AppSettings>>();
        // cookie domain is null in development, so add it so we can test it is added
        _appSettingsOptions.Value.Auth.CookieDomain = ".anarchychess.org";
        _settings = _appSettingsOptions.Value.Auth;

        _linkGenerator = Scope.ServiceProvider.GetRequiredService<LinkGenerator>();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void SetAuthCookies_sets_cookies_with_their_correct_attributes(bool isDevelopment)
    {
        var authCookieSetter = CreateAuthCookieSetter(isDevelopment);

        const string accessTokenValue = "access-token";
        const string refreshTokenValue = "refresh-token";

        var expectedAccessTokenCookie = CreateExpectedCookie(
            _settings.AccessTokenCookieName,
            accessTokenValue,
            _settings.AccessMaxAge
        );
        var expectedRefreshTokenCookie = CreateExpectedCookie(
            _settings.RefreshTokenCookieName,
            refreshTokenValue,
            _settings.RefreshMaxAge,
            path: _refreshPath
        );
        var expectedIsAuthedTokenCookie = CreateExpectedCookie(
            _settings.IsLoggedInCookieName,
            "true",
            _settings.RefreshMaxAge,
            httpOnly: false
        );

        var context = new DefaultHttpContext();

        authCookieSetter.SetAuthCookies(accessTokenValue, refreshTokenValue, context);

        var cookies = SetCookieHeaderValue.ParseList(context.Response.Headers.SetCookie);
        cookies.Should().HaveCount(3);
        AssertCookiesExists(cookies, expectedAccessTokenCookie);
        AssertCookiesExists(cookies, expectedRefreshTokenCookie);
        AssertCookiesExists(cookies, expectedIsAuthedTokenCookie);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void RemoveAuthCookies_only_deletes_auth_cookies(bool isDevelopment)
    {
        var authCookieSetter = CreateAuthCookieSetter(isDevelopment);
        var context = new DefaultHttpContext();

        var expectedAccessTokenCookie = CreateExpectedDeletedCookie(
            _settings.AccessTokenCookieName
        );
        var expectedRefreshTokenCookie = CreateExpectedDeletedCookie(
            _settings.RefreshTokenCookieName,
            _refreshPath
        );
        var expectedIsAuthedTokenCookie = CreateExpectedDeletedCookie(
            _settings.IsLoggedInCookieName
        );

        authCookieSetter.RemoveAuthCookies(context);

        var cookies = SetCookieHeaderValue.ParseList(context.Response.Headers.SetCookie);
        cookies.Should().HaveCount(3);
        AssertCookiesExists(cookies, expectedAccessTokenCookie);
        AssertCookiesExists(cookies, expectedRefreshTokenCookie);
        AssertCookiesExists(cookies, expectedIsAuthedTokenCookie);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void SetGuestCookie_sets_access_token_cookie(bool isDevelopment)
    {
        var authCookieSetter = CreateAuthCookieSetter(isDevelopment);
        const string accessTokenValue = "guest-access-token";

        var expectedCookie = CreateExpectedCookie(
            _settings.AccessTokenCookieName,
            accessTokenValue
        );

        var context = new DefaultHttpContext();
        authCookieSetter.SetGuestCookie(accessTokenValue, context);

        var cookies = SetCookieHeaderValue.ParseList(context.Response.Headers.SetCookie);
        cookies
            .Should()
            .ContainSingle(x => x.Name == _settings.AccessTokenCookieName)
            .Which.Should()
            .BeEquivalentTo(expectedCookie);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void SetAuthFailureCookie_sets_correct_error(bool isDevelopment)
    {
        var authCookieSetter = CreateAuthCookieSetter(isDevelopment);

        var error = AuthErrors.UserBanned;
        var expectedCookie = CreateExpectedCookie(
            _settings.AuthFailureCookieName,
            error.Code,
            httpOnly: false
        );

        var context = new DefaultHttpContext();
        authCookieSetter.SetAuthFailureCookie(error, context);

        var cookies = SetCookieHeaderValue.ParseList(context.Response.Headers.SetCookie);
        cookies
            .Should()
            .ContainSingle(x => x.Name == _settings.AuthFailureCookieName)
            .Which.Should()
            .BeEquivalentTo(expectedCookie);
    }

    private AuthCookieSetter CreateAuthCookieSetter(bool isDevelopment)
    {
        _webHostEnvironmentMock.EnvironmentName.Returns(
            isDevelopment ? Environments.Development : Environments.Production
        );
        return new AuthCookieSetter(_appSettingsOptions, _webHostEnvironmentMock, _linkGenerator);
    }

    private SetCookieHeaderValue CreateExpectedCookie(
        string name,
        string value,
        TimeSpan? maxAge = null,
        bool httpOnly = true,
        string path = "/"
    )
    {
        return new(name, value)
        {
            MaxAge = maxAge,
            HttpOnly = httpOnly,
            SameSite = SameSiteMode,
            Secure = true,
            Path = path,
            Domain = _settings.CookieDomain,
        };
    }

    private SetCookieHeaderValue CreateExpectedDeletedCookie(string name, string path = "/")
    {
        return new(name, "")
        {
            Secure = true,
            SameSite = SameSiteMode,
            Expires = new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero),
            Path = path,
            Domain = _settings.CookieDomain,
        };
    }

    private static void AssertCookiesExists(
        IEnumerable<SetCookieHeaderValue> cookies,
        SetCookieHeaderValue cookie
    )
    {
        cookies
            .Should()
            .ContainSingle(x => x.Name == cookie.Name)
            .Which.Should()
            .BeEquivalentTo(cookie);
    }
}
