using System.Security.Claims;
using System.Text.Json;
using Chess2.Api.Errors;
using Chess2.Api.Extensions;
using Chess2.Api.Models.Entities;
using ErrorOr;
using Microsoft.AspNetCore.Identity;
using static OpenIddict.Client.WebIntegration.OpenIddictClientWebIntegrationConstants;

namespace Chess2.Api.Services.Auth.OAuthAuthenticators;

public class DiscordOAuthAuthenticator(
    ILogger<DiscordOAuthAuthenticator> logger,
    IAuthService authService,
    UserManager<AuthedUser> userManager
) : IOAuthAuthenticator
{
    public string Provider => Providers.Discord;

    private readonly ILogger<DiscordOAuthAuthenticator> _logger = logger;
    private readonly IAuthService _authService = authService;
    private readonly UserManager<AuthedUser> _userManager = userManager;

    public async Task<ErrorOr<AuthedUser>> AuthenticateAsync(ClaimsPrincipal claimsPrincipal)
    {
        var discordUserIdResult = GetDiscordUserId(claimsPrincipal);
        if (discordUserIdResult.IsError)
            return discordUserIdResult.Errors;
        var discordUserId = discordUserIdResult.Value;

        var existingLogin = await _userManager.FindByLoginAsync(Provider, discordUserId);
        if (existingLogin is not null)
            return existingLogin;

        var signupResult = await _authService.SignupAsync("test");
        if (signupResult.IsError)
            return signupResult.Errors;
        var user = signupResult.Value;

        await _userManager.AddLoginAsync(
            user,
            new UserLoginInfo(Provider, discordUserId, Provider)
        );

        return user;
    }

    private ErrorOr<string> GetDiscordUserId(ClaimsPrincipal claimsPrincipal)
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
