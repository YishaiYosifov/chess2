using System.Net;
using System.Security.Claims;
using AnarchyChess.Api.Auth.Services;
using AnarchyChess.Api.Profile.Entities;
using AnarchyChess.Api.TestInfrastructure;
using AnarchyChess.Api.TestInfrastructure.Fakes;
using AnarchyChess.Api.TestInfrastructure.Utils;
using AwesomeAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using static OpenIddict.Client.WebIntegration.OpenIddictClientWebIntegrationConstants;

namespace AnarchyChess.Api.Integration.Tests.AuthTests;

public class OAuthServiceTests : BaseIntegrationTest
{
    private readonly IAuthenticationService _authenticationServiceMock =
        Substitute.For<IAuthenticationService>();
    private readonly IServiceProvider _httpContextServiceProviderMock =
        Substitute.For<IServiceProvider>();
    private readonly DefaultHttpContext _httpContext;

    private readonly IOAuthService _oauthService;
    private readonly UserManager<AuthedUser> _userManager;

    private readonly IPAddress _ipAddress = IPAddress.Parse("8.8.8.8");
    private const string _countryCode = "US";

    public OAuthServiceTests(AnarchyChessWebApplicationFactory factory)
        : base(factory)
    {
        _httpContextServiceProviderMock
            .GetService(typeof(IAuthenticationService))
            .Returns(_authenticationServiceMock);
        _httpContext = new() { RequestServices = _httpContextServiceProviderMock };
        _httpContext.Connection.RemoteIpAddress = _ipAddress;

        _oauthService = Scope.ServiceProvider.GetRequiredService<IOAuthService>();
        _userManager = Scope.ServiceProvider.GetRequiredService<UserManager<AuthedUser>>();
    }

    private async Task<AuthedUser> TestAuthenticateAsync(string provider, Claim claim)
    {
        var ticket = ClaimUtils.CreateAuthenticationTicket(claim);
        _authenticationServiceMock
            .AuthenticateAsync(Arg.Any<HttpContext>(), Arg.Any<string>())
            .Returns(AuthenticateResult.Success(ticket));

        var result = await _oauthService.AuthenticateAsync(provider, _httpContext);

        result.IsError.Should().BeFalse();
        var tokens = result.Value;

        AuthUtils.AuthenticateWithTokens(ApiClient, tokens.AccessToken, tokens.RefreshToken);
        await AuthTestUtils.AssertAuthenticated(ApiClient);

        var user = await _userManager.Users.SingleAsync();
        return user;
    }

    [Fact]
    public async Task AuthenticateAsync_Google_creates_user_with_email()
    {
        const string email = "test@email.com";
        Claim claim = new(ClaimTypes.Email, email);

        var user = await TestAuthenticateAsync(Providers.Google, claim);
        user.Email.Should().Be(email);
        user.UserName?.Length.Should().BeGreaterThan(10);
        user.CountryCode.Should().Be(_countryCode);
    }

    [Fact]
    public async Task AuthenticateAsync_Discord_creates_user_with_username()
    {
        const string userJson = "{\"id\":\"123123\"}";
        Claim claim = new("user", userJson);

        var user = await TestAuthenticateAsync(Providers.Discord, claim);
        user.Email.Should().BeNull();
        user.UserName?.Length.Should().BeGreaterThan(10);
        user.CountryCode.Should().Be(_countryCode);
    }

    [Fact]
    public async Task AuthenticateAsync_with_existing_user_just_logs_in()
    {
        var user = new AuthedUserFaker().Generate();
        await DbContext.AddAsync(user, CT);
        await DbContext.SaveChangesAsync(CT);

        var ticket = ClaimUtils.CreateAuthenticationTicket(
            new Claim(ClaimTypes.Email, user.Email!)
        );
        _authenticationServiceMock
            .AuthenticateAsync(Arg.Any<HttpContext>(), Arg.Any<string>())
            .Returns(AuthenticateResult.Success(ticket));
        await _userManager.AddLoginAsync(
            user,
            new(Providers.Google, user.Email!, Providers.Google)
        );

        var result = await _oauthService.AuthenticateAsync(Providers.Google, _httpContext, CT);
        result.IsError.Should().BeFalse();
        var tokens = result.Value;

        AuthUtils.AuthenticateWithTokens(ApiClient, tokens.AccessToken, tokens.RefreshToken);
        await AuthTestUtils.AssertAuthenticated(ApiClient);

        var users = await _userManager.Users.ToListAsync(CT);
        users.Should().HaveCount(1);
    }
}
