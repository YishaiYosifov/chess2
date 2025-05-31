using System.Security.Claims;
using Chess2.Api.Auth.Services;
using Chess2.Api.TestInfrastructure;
using Chess2.Api.TestInfrastructure.Fakes;
using Chess2.Api.TestInfrastructure.Utils;
using Chess2.Api.Users.Entities;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using static OpenIddict.Client.WebIntegration.OpenIddictClientWebIntegrationConstants;

namespace Chess2.Api.Integration.Tests;

public class OAuthServiceTests : BaseIntegrationTest
{
    private readonly IAuthenticationService _authenticationServiceMock =
        Substitute.For<IAuthenticationService>();
    private readonly IServiceProvider _httpContextServiceProviderMock =
        Substitute.For<IServiceProvider>();
    private readonly DefaultHttpContext _httpContext;

    private readonly IOAuthService _oauthService;
    private readonly UserManager<AuthedUser> _userManager;

    public OAuthServiceTests(Chess2WebApplicationFactory factory)
        : base(factory)
    {
        _httpContextServiceProviderMock
            .GetService(typeof(IAuthenticationService))
            .Returns(_authenticationServiceMock);

        _oauthService = Scope.ServiceProvider.GetRequiredService<IOAuthService>();
        _userManager = Scope.ServiceProvider.GetRequiredService<UserManager<AuthedUser>>();
        _httpContext = new DefaultHttpContext()
        {
            RequestServices = _httpContextServiceProviderMock,
        };
    }

    private async Task TestAuthenticateAsync(
        string provider,
        Claim claim,
        Action<AuthedUser> userCreationAssertion
    )
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
        userCreationAssertion(user);
    }

    [Fact]
    public async Task AuthenticateAsync_Google_creates_user_with_email()
    {
        const string email = "test@email.com";
        var claim = new Claim(ClaimTypes.Email, email);
        await TestAuthenticateAsync(
            Providers.Google,
            claim,
            user =>
            {
                user.Email.Should().Be(email);
                user.UserName?.Length.Should().BeGreaterThan(10);
            }
        );
    }

    [Fact]
    public async Task AuthenticateAsync_Discord_creates_user_with_username()
    {
        const string userJson = "{\"id\":\"123123\"}";
        var claim = new Claim("user", userJson);
        await TestAuthenticateAsync(
            Providers.Discord,
            claim,
            user =>
            {
                user.Email.Should().BeNull();
                user.UserName?.Length.Should().BeGreaterThan(10);
            }
        );
    }

    [Fact]
    public async Task AuthenticateAsync_with_existing_user_just_logs_in()
    {
        var user = await FakerUtils.StoreFakerAsync(DbContext, new AuthedUserFaker());
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

        var result = await _oauthService.AuthenticateAsync(Providers.Google, _httpContext);
        result.IsError.Should().BeFalse();
        var tokens = result.Value;

        AuthUtils.AuthenticateWithTokens(ApiClient, tokens.AccessToken, tokens.RefreshToken);
        await AuthTestUtils.AssertAuthenticated(ApiClient);

        var users = await _userManager.Users.ToListAsync();
        users.Should().HaveCount(1);
    }
}
