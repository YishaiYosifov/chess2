using System.Security.Claims;
using AnarchyChess.Api.Auth.Controllers;
using AnarchyChess.Api.Auth.Errors;
using AnarchyChess.Api.Auth.Services;
using AnarchyChess.Api.Shared.Models;
using AnarchyChess.Api.TestInfrastructure;
using AnarchyChess.Api.TestInfrastructure.Utils;
using AwesomeAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using NSubstitute;

namespace AnarchyChess.Api.Integration.Tests.AuthTests;

/// <summary>
/// Not a functional test because I can't be asked to try to deal with
/// openiddict middleware trying to verify my token :D
/// </summary>
public class OAuthControllerTests : BaseIntegrationTest
{
    private readonly OAuthController _controller;
    private readonly AuthSettings _settings;

    private readonly IAuthenticationService _authenticationServiceMock =
        Substitute.For<IAuthenticationService>();
    private readonly IServiceProvider _httpContextServiceProviderMock =
        Substitute.For<IServiceProvider>();

    private DefaultHttpContext _httpContext;

    private const string Email = "test@example.com";

    public OAuthControllerTests(AnarchyChessWebApplicationFactory factory)
        : base(factory)
    {
        Claim claim = new(ClaimTypes.Email, Email);
        var ticket = ClaimUtils.CreateAuthenticationTicket(claim);
        _authenticationServiceMock
            .AuthenticateAsync(Arg.Any<HttpContext>(), Arg.Any<string>())
            .Returns(AuthenticateResult.Success(ticket));

        _httpContextServiceProviderMock
            .GetService(typeof(IAuthenticationService))
            .Returns(_authenticationServiceMock);

        var settings = Scope.ServiceProvider.GetRequiredService<IOptions<AppSettings>>();
        _controller = new(
            Scope.ServiceProvider.GetRequiredService<ILogger<OAuthController>>(),
            Scope.ServiceProvider.GetRequiredService<IOAuthService>(),
            Scope.ServiceProvider.GetRequiredService<IAuthCookieSetter>(),
            Scope.ServiceProvider.GetRequiredService<IOAuthProviderNameNormalizer>(),
            settings
        );
        _settings = settings.Value.Auth;

        _httpContext = ResetHttpContext();
    }

    private DefaultHttpContext ResetHttpContext()
    {
        _httpContext = new() { RequestServices = _httpContextServiceProviderMock };
        _controller.ControllerContext = new() { HttpContext = _httpContext };
        return _httpContext;
    }

    private async Task AssertAuthenticatedAsync()
    {
        ApiClient.Client.DefaultRequestHeaders.Add(
            "Cookie",
            [.. _httpContext.Response.Headers.SetCookie]
        );
        await AuthTestUtils.AssertAuthenticated(ApiClient);
    }

    [Fact]
    public async Task OAuthCallback_sets_auth_cookies_and_redirects_on_account_creation()
    {
        var response = await _controller.OAuthCallback("google", CT);

        var redirect = response.Should().BeOfType<RedirectResult>().Subject;
        redirect.Url.Should().Be(_settings.OAuthRedirectUrl);
        await AssertAuthenticatedAsync();
    }

    [Fact]
    public async Task OAuthCallback_sets_auth_cookies_and_redirects_on_existing_account()
    {
        await _controller.OAuthCallback("google", CT);

        ResetHttpContext();
        var response = await _controller.OAuthCallback("google", CT);

        var redirect = response.Should().BeOfType<RedirectResult>().Subject;
        redirect.Url.Should().Be(_settings.OAuthRedirectUrl);
        await AssertAuthenticatedAsync();
    }

    [Fact]
    public async Task OAuthCallback_sets_auth_failure_cookie_and_redirects_to_login_on_error()
    {
        await _controller.OAuthCallback("google", CT);
        var user = await DbContext.Users.Where(x => x.Email == Email).FirstAsync(CT);
        user.IsBanned = true;
        await DbContext.SaveChangesAsync(CT);
        ResetHttpContext();

        var response = await _controller.OAuthCallback("google", CT);

        var redirect = response.Should().BeOfType<RedirectResult>().Subject;
        redirect.Url.Should().Be(_settings.LoginPageUrl);

        var authFailedCookie = SetCookieHeaderValue
            .ParseList(_httpContext.Response.Headers.SetCookie)
            .FirstOrDefault(x => x.Name == _settings.AuthFailureCookieName);
        authFailedCookie.Should().NotBeNull();
        authFailedCookie.Value.ToString().Should().Be(AuthErrors.UserBanned.Code);
    }
}
