using Chess2.Api.Auth.Services;
using Chess2.Api.Shared.Models;
using Chess2.Api.TestInfrastructure;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using NSubstitute;
using SameSiteMode = Microsoft.Net.Http.Headers.SameSiteMode;

namespace Chess2.Api.Integration.Tests.AuthTests;

public class AuthCookieSetterTests : BaseIntegrationTest
{
    private readonly JwtSettings _jwtSettings;
    private readonly IOptions<AppSettings> _appSettingsOptions;

    private readonly IWebHostEnvironment _webHostEnvironmentMock =
        Substitute.For<IWebHostEnvironment>();
    private readonly LinkGenerator _linkGenerator;

    private SameSiteMode SameSiteMode =>
        _webHostEnvironmentMock.IsDevelopment() ? SameSiteMode.None : SameSiteMode.Strict;

    private const string _refreshPath = "/api/auth/refresh";

    public AuthCookieSetterTests(Chess2WebApplicationFactory factory)
        : base(factory)
    {
        _appSettingsOptions = Scope.ServiceProvider.GetRequiredService<IOptions<AppSettings>>();
        _jwtSettings = _appSettingsOptions.Value.Jwt;

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
            _jwtSettings.AccessTokenCookieName,
            accessTokenValue,
            _jwtSettings.AccessMaxAge
        );
        var expectedRefreshTokenCookie = CreateExpectedCookie(
            _jwtSettings.RefreshTokenCookieName,
            refreshTokenValue,
            _jwtSettings.RefreshMaxAge,
            path: _refreshPath
        );
        var expectedIsAuthedTokenCookie = CreateExpectedCookie(
            _jwtSettings.IsLoggedInCookieName,
            "true",
            _jwtSettings.RefreshMaxAge,
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
            _jwtSettings.AccessTokenCookieName
        );
        var expectedRefreshTokenCookie = CreateExpectedDeletedCookie(
            _jwtSettings.RefreshTokenCookieName,
            _refreshPath
        );
        var expectedIsAuthedTokenCookie = CreateExpectedDeletedCookie(
            _jwtSettings.IsLoggedInCookieName
        );

        authCookieSetter.RemoveAuthCookies(context);

        var cookies = SetCookieHeaderValue.ParseList(context.Response.Headers.SetCookie);
        cookies.Should().HaveCount(3);
        AssertCookiesExists(cookies, expectedAccessTokenCookie);
        AssertCookiesExists(cookies, expectedRefreshTokenCookie);
        AssertCookiesExists(cookies, expectedIsAuthedTokenCookie);
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
        TimeSpan maxAge,
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
