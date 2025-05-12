using System.Security.Claims;
using Chess2.Api.Models.Entities;
using ErrorOr;

namespace Chess2.Api.Services.Auth.OAuthAuthenticators;

public interface IOAuthAuthenticator
{
    string Provider { get; }

    /// <summary>
    /// Authenticate the user using the claims principal from the OAuth provider
    /// </summary>
    public Task<ErrorOr<AuthedUser>> AuthenticateAsync(ClaimsPrincipal claimsPrincipal);
}
