using System.Security.Claims;
using Chess2.Api.Errors;
using Chess2.Api.Models.Entities;
using Chess2.Api.Services.Auth;
using Chess2.Api.Services.Auth.OAuthAuthenticators;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using NSubstitute;
using static OpenIddict.Client.WebIntegration.OpenIddictClientWebIntegrationConstants;

namespace Chess2.Api.Unit.Tests.OAuthTests.OAuthAuthenticatorTests;

public class DiscordOAuthAuthenticatorTests : BaseUnitTest
{
    private readonly IAuthService _authServiceMock = Substitute.For<IAuthService>();
    private readonly UserManager<AuthedUser> _userManagerMock;

    private readonly DiscordOAuthAuthenticator _authenticator;

    public DiscordOAuthAuthenticatorTests()
    {
        _userManagerMock = Substitute.ForPartsOf<UserManager<AuthedUser>>(
            Substitute.For<IUserLoginStore<AuthedUser>>(),
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null
        );
        _authenticator = new(
            Substitute.For<ILogger<DiscordOAuthAuthenticator>>(),
            _authServiceMock,
            _userManagerMock
        );
    }

    private static ClaimsPrincipal CreateClaimsPrincipalWithUserId(string? userId)
    {
        var userJson = userId != null ? $"{{\"id\":\"{userId}\"}}" : "{}";
        var claims = new List<Claim> { new("user", userJson) };
        var identity = new ClaimsIdentity(claims, "TestAuthType");
        return new ClaimsPrincipal(identity);
    }

    [Fact]
    public void Provider_is_set_to_Discord_constant()
    {
        var provider = _authenticator.Provider;
        provider.Should().Be(Providers.Discord);
    }

    [Fact]
    public async Task Authenticating_an_existing_user_returns_that_user()
    {
        var userId = "123456";
        var user = new AuthedUser();
        _userManagerMock.FindByLoginAsync(Providers.Discord, userId).Returns(user);

        var principal = CreateClaimsPrincipalWithUserId(userId);

        var result = await _authenticator.AuthenticateAsync(principal);

        result.IsError.Should().BeFalse();
        result.Value.Should().BeEquivalentTo(user);
    }

    [Fact]
    public async Task Authenticating_a_non_existing_user_creates_it_and_add_a_login()
    {
        var userId = "654321";
        var user = new AuthedUser();
        _userManagerMock.FindByLoginAsync(Providers.Discord, userId).Returns((AuthedUser?)null);
        _authServiceMock.SignupAsync(Arg.Any<string>(), default, default).Returns(user);
        _userManagerMock
            .AddLoginAsync(user, Arg.Any<UserLoginInfo>())
            .Returns(IdentityResult.Success);

        var principal = CreateClaimsPrincipalWithUserId(userId);

        var result = await _authenticator.AuthenticateAsync(principal);

        result.IsError.Should().BeFalse();
        result.Value.Should().BeEquivalentTo(user);
        await _userManagerMock
            .Received(1)
            .AddLoginAsync(
                user,
                Arg.Is<UserLoginInfo>(l =>
                    l.ProviderKey == userId
                    && l.LoginProvider == Providers.Discord
                    && l.ProviderDisplayName == Providers.Discord
                )
            );
    }

    [Fact]
    public async Task Errors_from_signup_are_forwarded()
    {
        var userId = "999999";
        var error = AuthErrors.OAuthInvalid;
        _userManagerMock.FindByLoginAsync(Providers.Discord, userId).Returns((AuthedUser?)null);
        _authServiceMock.SignupAsync(Arg.Any<string>()).Returns(error);
        var principal = CreateClaimsPrincipalWithUserId(userId);

        var result = await _authenticator.AuthenticateAsync(principal);

        result.IsError.Should().BeTrue();
        result.Errors.Should().ContainSingle();
        result.Errors.Single().Should().Be(error);
    }

    [Fact]
    public async Task Authenticating_without_a_user_claim_returns_an_error()
    {
        var principal = new ClaimsPrincipal(new ClaimsIdentity());

        var result = await _authenticator.AuthenticateAsync(principal);

        result.IsError.Should().BeTrue();
    }

    [Fact]
    public async Task Authenticating_with_an_invalid_user_claim_returns_an_error()
    {
        var userJson = "{}";
        var claims = new List<Claim> { new("user", userJson) };
        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var principal = new ClaimsPrincipal(identity);

        var result = await _authenticator.AuthenticateAsync(principal);

        result.IsError.Should().BeTrue();
    }
}
