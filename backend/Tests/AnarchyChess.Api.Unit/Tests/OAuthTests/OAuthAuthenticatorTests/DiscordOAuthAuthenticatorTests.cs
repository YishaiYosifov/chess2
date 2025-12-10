using System.Security.Claims;
using AnarchyChess.Api.Auth.Models;
using AnarchyChess.Api.Auth.OAuthAuthenticators;
using Microsoft.Extensions.Logging;
using NSubstitute;
using static OpenIddict.Client.WebIntegration.OpenIddictClientWebIntegrationConstants;

namespace AnarchyChess.Api.Unit.Tests.OAuthTests.OAuthAuthenticatorTests;

public class DiscordOAuthAuthenticatorTests : BaseOAuthAuthenticatorTests<DiscordOAuthAuthenticator>
{
    protected override string Provider => Providers.Discord;

    protected override DiscordOAuthAuthenticator CreateAuthenticator() =>
        new(Substitute.For<ILogger<DiscordOAuthAuthenticator>>());

    protected override (Claim Claim, OAuthIdentity ExpectedOAuthIdentity) GetClaim(
        string providerKey
    ) =>
        (
            new Claim("user", $"{{\"id\":\"{providerKey}\"}}"),
            new OAuthIdentity(ProviderKey: providerKey, Email: null)
        );
}
