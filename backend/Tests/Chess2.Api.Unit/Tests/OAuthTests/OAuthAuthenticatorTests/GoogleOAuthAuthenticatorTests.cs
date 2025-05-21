using System.Security.Claims;
using Chess2.Api.Auth.Services.OAuthAuthenticators;
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
        const string username = "test-username-1234";
        const string email = "test@email.com";
        UsernameGeneratorMock.GenerateUniqueUsernameAsync().Returns(username);

        var user = new AuthedUserFaker()
            .RuleFor(x => x.UserName, username)
            .RuleFor(x => x.Email, email)
            .Generate();
        AuthServiceMock.SignupAsync(username, email, default).Returns(user);

        var result = await Authenticator.SignUserUpAsync(new(), email);

        result.IsError.Should().BeFalse();
        result.Value.Should().BeEquivalentTo(user);
    }
}
