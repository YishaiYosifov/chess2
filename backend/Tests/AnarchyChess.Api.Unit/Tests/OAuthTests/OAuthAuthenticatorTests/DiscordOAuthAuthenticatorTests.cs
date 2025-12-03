using System.Security.Claims;
using AnarchyChess.Api.Auth.Services.OAuthAuthenticators;
using AnarchyChess.Api.TestInfrastructure.Fakes;
using AwesomeAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using static OpenIddict.Client.WebIntegration.OpenIddictClientWebIntegrationConstants;

namespace AnarchyChess.Api.Unit.Tests.OAuthTests.OAuthAuthenticatorTests;

public class DiscordOAuthAuthenticatorTests : BaseOAuthAuthenticatorTests<DiscordOAuthAuthenticator>
{
    protected override string Provider => Providers.Discord;

    protected override DiscordOAuthAuthenticator CreateAuthenticator() =>
        new(
            Substitute.For<ILogger<DiscordOAuthAuthenticator>>(),
            AuthServiceMock,
            UsernameGeneratorMock
        );

    protected override Claim CreateProviderKeyClaim(string? key)
    {
        var userJson = key != null ? $"{{\"id\":\"{key}\"}}" : "{}";
        return new("user", userJson);
    }

    [Fact]
    public void GetProviderKey_with_missing_user_id_returns_an_error()
    {
        var claim = CreateProviderKeyClaim(null);
        var identity = new ClaimsIdentity([claim], "TestAuthType");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        var providerKeyResult = Authenticator.GetProviderKey(claimsPrincipal);

        providerKeyResult.IsError.Should().BeTrue();
    }

    [Fact]
    public async Task SignUserUpAsync_create_a_user_correctly()
    {
        const string username = "test-username-1234";
        UsernameGeneratorMock.GenerateUniqueUsernameAsync().Returns(username);

        var user = new AuthedUserFaker().RuleFor(x => x.UserName, username).Generate();
        AuthServiceMock.SignupAsync(username, null, null).Returns(user);

        var result = await Authenticator.SignUserUpAsync(new(), "discord-id-1234");

        result.IsError.Should().BeFalse();
        result.Value.Should().BeEquivalentTo(user);
    }
}
