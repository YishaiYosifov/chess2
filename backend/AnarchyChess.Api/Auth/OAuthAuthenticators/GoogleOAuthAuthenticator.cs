using System.Security.Claims;
using AnarchyChess.Api.Auth.Errors;
using AnarchyChess.Api.Auth.Models;
using ErrorOr;
using OpenIddict.Abstractions;
using static OpenIddict.Client.WebIntegration.OpenIddictClientWebIntegrationConstants;

namespace AnarchyChess.Api.Auth.OAuthAuthenticators;

public class GoogleOAuthAuthenticator(ILogger<GoogleOAuthAuthenticator> logger)
    : IOAuthAuthenticator
{
    private readonly ILogger<GoogleOAuthAuthenticator> _logger = logger;

    public string Provider => Providers.Google;

    public ErrorOr<OAuthIdentity> ExtractOAuthIdentity(ClaimsPrincipal claimsPrincipal)
    {
        var email = claimsPrincipal.GetClaim(ClaimTypes.Email);
        if (email is null)
        {
            _logger.LogWarning("Could not get email claim from google claims principal");
            return AuthErrors.OAuthInvalid;
        }
        return new OAuthIdentity(ProviderKey: email, Email: email);
    }
}
