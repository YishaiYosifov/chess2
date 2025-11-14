using System.Security.Claims;
using AnarchyChess.Api.Auth.Services;
using AnarchyChess.Api.Auth.Services.OAuthAuthenticators;
using AnarchyChess.Api.Profile.Services;
using ErrorOr;
using FluentAssertions;
using NSubstitute;

namespace AnarchyChess.Api.Unit.Tests.OAuthTests.OAuthAuthenticatorTests;

public abstract class BaseOAuthAuthenticatorTests<TAuthenticator> : BaseUnitTest
    where TAuthenticator : IOAuthAuthenticator
{
    protected readonly IAuthService AuthServiceMock = Substitute.For<IAuthService>();
    protected readonly IUsernameGenerator UsernameGeneratorMock =
        Substitute.For<IUsernameGenerator>();

    protected readonly TAuthenticator Authenticator;

    protected abstract string Provider { get; }

    public BaseOAuthAuthenticatorTests()
    {
        Authenticator = CreateAuthenticator();
    }

    [Fact]
    public void Provider_is_set_to_expected_constant()
    {
        var provider = Authenticator.Provider;
        provider.Should().Be(Provider);
    }

    [Fact]
    public void GetProviderKey_returns_the_correct_value()
    {
        var key = "test";
        var claim = CreateProviderKeyClaim(key);
        var identity = new ClaimsIdentity([claim], "TestAuthType");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        var providerKeyResult = Authenticator.GetProviderKey(claimsPrincipal);

        providerKeyResult.IsError.Should().BeFalse();
        providerKeyResult.Value.Should().Be(key);
    }

    [Fact]
    public void GetProviderKey_returns_an_error_when_the_claim_is_missing()
    {
        var identity = new ClaimsIdentity([], "TestAuthType");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        var providerKeyResult = Authenticator.GetProviderKey(claimsPrincipal);

        providerKeyResult.IsError.Should().BeTrue();
    }

    [Fact]
    public async Task SignUserUpAsync_forwards_errors_from_signup()
    {
        var error = Error.Failure();
        AuthServiceMock.SignupAsync(Arg.Any<string>(), default, default).ReturnsForAnyArgs(error);

        var result = await Authenticator.SignUserUpAsync(new(), "test");

        result.IsError.Should().BeTrue();
        result.Errors.Single().Should().Be(error);
    }

    protected abstract TAuthenticator CreateAuthenticator();
    protected abstract Claim CreateProviderKeyClaim(string key);
}
