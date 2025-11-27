using System.Security.Claims;
using AnarchyChess.Api.Profile.Models;
using OpenIddict.Abstractions;

namespace AnarchyChess.Api.Auth.Services;

public interface IGuestService
{
    string CreateGuestUser();

    bool IsGuest(ClaimsPrincipal? userClaims);
}

public class GuestService(ILogger<GuestService> logger, ITokenProvider tokenProvider)
    : IGuestService
{
    private readonly ITokenProvider _tokenProvider = tokenProvider;
    private readonly ILogger<GuestService> _logger = logger;

    /// <summary>
    /// Create a stateless guest user id and its access token
    /// </summary>
    /// <returns>The guest access token</returns>
    public string CreateGuestUser()
    {
        var id = UserId.Guest();
        var accessToken = _tokenProvider.GenerateGuestToken(id);
        _logger.LogInformation("Created guest user with id {Id}", id);
        return accessToken;
    }

    /// <summary>
    /// Find whether the user claims indicate the user is a guest or not
    /// </summary>
    public bool IsGuest(ClaimsPrincipal? userClaims)
    {
        if (userClaims is null)
            return false;

        var isAnnonymous = userClaims.GetClaim(ClaimTypes.Anonymous);
        var isGuest = isAnnonymous is not null && isAnnonymous == "true";
        return isGuest;
    }
}
