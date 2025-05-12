using System.Security.Claims;
using System.Text.Json;
using Chess2.Api.Errors;
using Chess2.Api.Extensions;
using Chess2.Api.Models.Entities;
using ErrorOr;
using static OpenIddict.Client.WebIntegration.OpenIddictClientWebIntegrationConstants;

namespace Chess2.Api.Services.Auth.OAuthAuthenticators;

public class DiscordOAuthAuthenticator(
    ILogger<DiscordOAuthAuthenticator> logger,
    IAuthService authService
) : IOAuthAuthenticator
{
    public string Provider => Providers.Discord;

    private readonly ILogger<DiscordOAuthAuthenticator> _logger = logger;
    private readonly IAuthService _authService = authService;

    public async Task<ErrorOr<AuthedUser>> SignUserUp(
        ClaimsPrincipal claimsPrincipal,
        string providerKey
    )
    {
        var signupResult = await _authService.SignupAsync("test");
        return signupResult;
    }

    public ErrorOr<string> GetProviderKey(ClaimsPrincipal claimsPrincipal)
    {
        var userClaimResult = claimsPrincipal.GetClaim("user");
        if (userClaimResult.IsError)
        {
            _logger.LogWarning("Could not get user claim from discord claims principal");
            return userClaimResult.Errors;
        }
        var userClaim = userClaimResult.Value;

        using var doc = JsonDocument.Parse(userClaim.Value);
        if (!doc.RootElement.TryGetProperty("id", out var userIdElement))
        {
            _logger.LogWarning("Could not get user id from discord claims principal");
            return AuthErrors.OAuthInvalid;
        }

        var userId = userIdElement.GetString();
        if (userId is null)
        {
            _logger.LogWarning("User id was null in discord claims principal");
            return AuthErrors.OAuthInvalid;
        }

        return userId;
    }
}
