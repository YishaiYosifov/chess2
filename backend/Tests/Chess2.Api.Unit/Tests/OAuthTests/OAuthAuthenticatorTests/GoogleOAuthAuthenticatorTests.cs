using System.Security.Claims;
using Chess2.Api.Services.Auth.OAuthAuthenticators;
using Chess2.Api.TestInfrastructure.Fakes;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using static OpenIddict.Client.WebIntegration.OpenIddictClientWebIntegrationConstants;

namespace Chess2.Api.Unit.Tests.OAuthTests.OAuthAuthenticatorTests;

public class GoogleOAuthAuthenticatorTests : BaseOAuthAuthenticatorTests<GoogleOAuthAuthenticator>
{
    private readonly ILogger<GoogleOAuthAuthenticator> _loggerMock = Substitute.For<
        ILogger<GoogleOAuthAuthenticator>
    >();

    protected override string Provider => Providers.Google;

    protected override GoogleOAuthAuthenticator CreateAuthenticator() =>
        new(_loggerMock, AuthServiceMock, UsernameGeneratorMock);

    protected override Claim CreateProviderKeyClaim(string key) => new(ClaimTypes.Email, key);

    [Fact]
    public async Task SignUserUpAsync_create_a_user_correctly()
    {
        var user = new AuthedUserFaker().Generate();
        var email = "test@email.com";
        AuthServiceMock.SignupAsync(email, email, default).Returns(user);

        var result = await Authenticator.SignUserUpAsync(new(), email);

        result.IsError.Should().BeFalse();
        result.Value.Should().BeEquivalentTo(user);
    }
}
