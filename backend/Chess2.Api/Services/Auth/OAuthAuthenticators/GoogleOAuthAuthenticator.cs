using System.Security.Claims;
using Chess2.Api.Errors;
using Chess2.Api.Models.Entities;
using ErrorOr;
using OpenIddict.Abstractions;
using static OpenIddict.Client.WebIntegration.OpenIddictClientWebIntegrationConstants;

namespace Chess2.Api.Services.Auth.OAuthAuthenticators;

public class GoogleOAuthAuthenticator(
    ILogger<GoogleOAuthAuthenticator> logger,
    IAuthService authService
) : IOAuthAuthenticator
{
    private readonly ILogger<GoogleOAuthAuthenticator> _logger = logger;
    private readonly IAuthService _authService = authService;

    public string Provider => Providers.Google;

    public async Task<ErrorOr<AuthedUser>> SignUserUpAsync(
        ClaimsPrincipal claimsPrincipal,
        string providerKey
    )
    {
        var signupResult = await _authService.SignupAsync(providerKey, providerKey);
        return signupResult;
    }

    public ErrorOr<string> GetProviderKey(ClaimsPrincipal claimsPrincipal)
    {
        var email = claimsPrincipal.GetClaim(ClaimTypes.Email);
        if (email is null)
        {
            _logger.LogWarning("Could not get email claim from google claims principal");
            return AuthErrors.OAuthInvalid;
        }
        return email;
    }
}
