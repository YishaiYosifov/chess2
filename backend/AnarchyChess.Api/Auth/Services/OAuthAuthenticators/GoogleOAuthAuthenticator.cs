using System.Security.Claims;
using AnarchyChess.Api.Auth.Errors;
using AnarchyChess.Api.Auth.Services;
using AnarchyChess.Api.Profile.Entities;
using AnarchyChess.Api.Profile.Services;
using ErrorOr;
using OpenIddict.Abstractions;
using static OpenIddict.Client.WebIntegration.OpenIddictClientWebIntegrationConstants;

namespace AnarchyChess.Api.Auth.Services.OAuthAuthenticators;

public class GoogleOAuthAuthenticator(
    ILogger<GoogleOAuthAuthenticator> logger,
    IAuthService authService,
    IUsernameGenerator usernameGenerator
) : IOAuthAuthenticator
{
    private readonly ILogger<GoogleOAuthAuthenticator> _logger = logger;
    private readonly IAuthService _authService = authService;
    private readonly IUsernameGenerator _usernameGenerator = usernameGenerator;

    public string Provider => Providers.Google;

    public async Task<ErrorOr<AuthedUser>> SignUserUpAsync(
        ClaimsPrincipal claimsPrincipal,
        string email
    )
    {
        var username = await _usernameGenerator.GenerateUniqueUsernameAsync();
        var signupResult = await _authService.SignupAsync(username, email);
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
