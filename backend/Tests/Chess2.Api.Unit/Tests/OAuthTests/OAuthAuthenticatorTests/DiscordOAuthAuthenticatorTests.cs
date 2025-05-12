using System.Security.Claims;
using Chess2.Api.Services.Auth.OAuthAuthenticators;
using Chess2.Api.TestInfrastructure.Fakes;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using static OpenIddict.Client.WebIntegration.OpenIddictClientWebIntegrationConstants;

namespace Chess2.Api.Unit.Tests.OAuthTests.OAuthAuthenticatorTests;

public class DiscordOAuthAuthenticatorTests : BaseOAuthAuthenticatorTests<DiscordOAuthAuthenticator>
{
    protected override string Provider => Providers.Discord;

    protected override DiscordOAuthAuthenticator CreateAuthenticator() =>
        new(Substitute.For<ILogger<DiscordOAuthAuthenticator>>(), AuthServiceMock);

    protected override Claim CreateProviderKeyClaim(string? key)
    {
        var userJson = key != null ? $"{{\"id\":\"{key}\"}}" : "{}";
        return new("user", userJson);
    }

    [Fact]
    public async Task User_is_created_correctly()
    {
        var user = new AuthedUserFaker().Generate();
        AuthServiceMock.SignupAsync("test", default, default).Returns(user);

        var result = await Authenticator.SignUserUp(new(), "");

        result.IsError.Should().BeFalse();
        result.Value.Should().BeEquivalentTo(user);
    }
}
