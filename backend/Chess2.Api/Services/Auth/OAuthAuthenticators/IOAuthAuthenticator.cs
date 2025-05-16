using System.Security.Claims;
using Chess2.Api.Models.Entities;
using ErrorOr;

namespace Chess2.Api.Services.Auth.OAuthAuthenticators;

public interface IOAuthAuthenticator
{
    string Provider { get; }

    Task<ErrorOr<AuthedUser>> SignUserUpAsync(ClaimsPrincipal claimsPrincipal, string providerKey);
    ErrorOr<string> GetProviderKey(ClaimsPrincipal claimsPrincipal);
}
