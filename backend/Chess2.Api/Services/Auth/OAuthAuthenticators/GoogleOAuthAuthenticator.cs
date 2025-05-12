using System.Security.Claims;
using Chess2.Api.Extensions;
using Chess2.Api.Models.Entities;
using ErrorOr;
using static OpenIddict.Client.WebIntegration.OpenIddictClientWebIntegrationConstants;

namespace Chess2.Api.Services.Auth.OAuthAuthenticators;

public class GoogleOAuthAuthenticator(IAuthService authService) : IOAuthAuthenticator
{
    private readonly IAuthService _authService = authService;

    public string Provider => Providers.Google;

    public async Task<ErrorOr<AuthedUser>> SignUserUp(
        ClaimsPrincipal claimsPrincipal,
        string providerKey
    )
    {
        var signupResult = await _authService.SignupAsync(providerKey, providerKey);
        return signupResult;
    }

    public ErrorOr<string> GetProviderKey(ClaimsPrincipal claimsPrincipal)
    {
        var claimEmailResult = claimsPrincipal.GetClaim(ClaimTypes.Email);
        if (claimEmailResult.IsError)
            return claimEmailResult.Errors;
        var emailClaim = claimEmailResult.Value;
        return emailClaim.Value;
    }
}
