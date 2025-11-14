using System.Security.Claims;
using AnarchyChess.Api.Profile.Entities;
using ErrorOr;

namespace AnarchyChess.Api.Auth.Services.OAuthAuthenticators;

public interface IOAuthAuthenticator
{
    string Provider { get; }

    Task<ErrorOr<AuthedUser>> SignUserUpAsync(ClaimsPrincipal claimsPrincipal, string providerKey);
    ErrorOr<string> GetProviderKey(ClaimsPrincipal claimsPrincipal);
}
