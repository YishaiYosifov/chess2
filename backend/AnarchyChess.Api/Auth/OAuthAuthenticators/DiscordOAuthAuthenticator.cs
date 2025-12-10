using System.Security.Claims;
using System.Text.Json;
using AnarchyChess.Api.Auth.Errors;
using AnarchyChess.Api.Auth.Models;
using ErrorOr;
using OpenIddict.Abstractions;
using static OpenIddict.Client.WebIntegration.OpenIddictClientWebIntegrationConstants;

namespace AnarchyChess.Api.Auth.OAuthAuthenticators;

public class DiscordOAuthAuthenticator(ILogger<DiscordOAuthAuthenticator> logger)
    : IOAuthAuthenticator
{
    public string Provider => Providers.Discord;

    private readonly ILogger<DiscordOAuthAuthenticator> _logger = logger;

    public ErrorOr<OAuthIdentity> ExtractOAuthIdentity(ClaimsPrincipal claimsPrincipal)
    {
        var userClaim = claimsPrincipal.GetClaim("user");
        if (userClaim is null)
        {
            _logger.LogWarning("Could not get user claim from discord claims principal");
            return AuthErrors.OAuthInvalid;
        }

        using var doc = JsonDocument.Parse(userClaim);
        if (!doc.RootElement.TryGetProperty("id", out var discordIdElement))
        {
            _logger.LogWarning("Could not get user id from discord claims principal");
            return AuthErrors.OAuthInvalid;
        }

        var discordId = discordIdElement.GetString();
        if (discordId is null)
        {
            _logger.LogWarning("User id was null in discord claims principal");
            return AuthErrors.OAuthInvalid;
        }

        return new OAuthIdentity(ProviderKey: discordId, Email: null);
    }
}
