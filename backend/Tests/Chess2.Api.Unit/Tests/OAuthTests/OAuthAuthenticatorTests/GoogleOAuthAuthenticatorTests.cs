using System.Security.Claims;
using Chess2.Api.Services.Auth.OAuthAuthenticators;
using Chess2.Api.TestInfrastructure.Fakes;
using FluentAssertions;
using NSubstitute;
using static OpenIddict.Client.WebIntegration.OpenIddictClientWebIntegrationConstants;

namespace Chess2.Api.Unit.Tests.OAuthTests.OAuthAuthenticatorTests;

public class GoogleOAuthAuthenticatorTests : BaseOAuthAuthenticatorTests<GoogleOAuthAuthenticator>
{
    protected override string Provider => Providers.Google;

    protected override GoogleOAuthAuthenticator CreateAuthenticator() => new(AuthServiceMock);

    protected override Claim? CreateProviderKeyClaim(string? key)
    {
        if (key is null)
            return null;
        return new(ClaimTypes.Email, key);
    }

    [Fact]
    public async Task User_is_created_correctly()
    {
        var user = new AuthedUserFaker().Generate();
        var email = "test@email.com";
        AuthServiceMock.SignupAsync(email, email, default).Returns(user);

        var result = await Authenticator.SignUserUp(new(), email);

        result.IsError.Should().BeFalse();
        result.Value.Should().BeEquivalentTo(user);
    }
}
