using System.Security.Claims;
using Chess2.Api.Services.Auth;
using Chess2.Api.Services.Auth.OAuthAuthenticators;
using ErrorOr;
using FluentAssertions;
using NSubstitute;

namespace Chess2.Api.Unit.Tests.OAuthTests.OAuthAuthenticatorTests;

public abstract class BaseOAuthAuthenticatorTests<TAuthenticator> : BaseUnitTest
    where TAuthenticator : IOAuthAuthenticator
{
    protected readonly IAuthService AuthServiceMock = Substitute.For<IAuthService>();
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

    [Theory]
    [InlineData("123123")]
    [InlineData(null)]
    public void Correct_provider_key_is_found(string? expectedProviderKey)
    {
        var claims = new List<Claim>();

        var claim = CreateProviderKeyClaim(expectedProviderKey);
        if (claim is not null)
            claims.Add(claim);

        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        var providerKeyResult = Authenticator.GetProviderKey(claimsPrincipal);

        if (expectedProviderKey is null)
        {
            providerKeyResult.IsError.Should().BeTrue();
        }
        else
        {
            providerKeyResult.IsError.Should().BeFalse();
            providerKeyResult.Value.Should().Be(expectedProviderKey);
        }
    }

    [Fact]
    public async Task Errors_from_signup_are_forwarded()
    {
        var error = Error.Failure();
        AuthServiceMock.SignupAsync(Arg.Any<string>(), default, default).ReturnsForAnyArgs(error);

        var result = await Authenticator.SignUserUp(new(), "test");

        result.IsError.Should().BeTrue();
        result.Errors.Single().Should().Be(error);
    }

    protected abstract TAuthenticator CreateAuthenticator();
    protected abstract Claim? CreateProviderKeyClaim(string? key);
}
