using System.Security.Claims;
using AnarchyChess.Api.Auth.Models;
using AnarchyChess.Api.Auth.OAuthAuthenticators;
using Microsoft.Extensions.Logging;
using NSubstitute;
using static OpenIddict.Client.WebIntegration.OpenIddictClientWebIntegrationConstants;

namespace AnarchyChess.Api.Unit.Tests.OAuthTests.OAuthAuthenticatorTests;

public class GoogleOAuthAuthenticatorTests : BaseOAuthAuthenticatorTests<GoogleOAuthAuthenticator>
{
    protected override string Provider => Providers.Google;

    protected override GoogleOAuthAuthenticator CreateAuthenticator() =>
        new(Substitute.For<ILogger<GoogleOAuthAuthenticator>>());

    protected override (Claim Claim, OAuthIdentity ExpectedOAuthIdentity) GetClaim(
        string providerKey
    ) => (new(ClaimTypes.Email, providerKey), new OAuthIdentity(providerKey, Email: providerKey));
}
