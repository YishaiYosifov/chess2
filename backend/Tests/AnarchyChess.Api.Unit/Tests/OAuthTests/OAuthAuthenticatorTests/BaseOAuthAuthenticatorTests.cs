using System.Security.Claims;
using AnarchyChess.Api.Auth.Errors;
using AnarchyChess.Api.Auth.Models;
using AnarchyChess.Api.Auth.OAuthAuthenticators;
using AwesomeAssertions;

namespace AnarchyChess.Api.Unit.Tests.OAuthTests.OAuthAuthenticatorTests;

public abstract class BaseOAuthAuthenticatorTests<TAuthenticator> : BaseUnitTest
    where TAuthenticator : IOAuthAuthenticator
{
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
    public void ExtractOAuthIdentity_returns_the_correct_provider_key()
    {
        string key = "test";
        var (claim, expectedOAuthIdentity) = GetClaim(key);
        ClaimsIdentity claimIdentity = new([claim], "TestAuthType");
        ClaimsPrincipal claimsPrincipal = new(claimIdentity);

        var result = Authenticator.ExtractOAuthIdentity(claimsPrincipal);

        result.IsError.Should().BeFalse();
        result.Value.Should().Be(expectedOAuthIdentity);
    }

    [Fact]
    public void ExtractOAuthIdentity_returns_an_error_when_the_provider_key_claim_is_missing()
    {
        ClaimsIdentity identity = new([], "TestAuthType");
        ClaimsPrincipal claimsPrincipal = new(identity);

        var providerKeyResult = Authenticator.ExtractOAuthIdentity(claimsPrincipal);

        providerKeyResult.IsError.Should().BeTrue();
        providerKeyResult.FirstError.Should().Be(AuthErrors.OAuthInvalid);
    }

    protected abstract TAuthenticator CreateAuthenticator();
    protected abstract (Claim Claim, OAuthIdentity ExpectedOAuthIdentity) GetClaim(
        string providerKey
    );
}
