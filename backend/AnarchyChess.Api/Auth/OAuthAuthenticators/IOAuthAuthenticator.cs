using System.Security.Claims;
using AnarchyChess.Api.Auth.Models;
using ErrorOr;

namespace AnarchyChess.Api.Auth.OAuthAuthenticators;

public interface IOAuthAuthenticator
{
    string Provider { get; }

    ErrorOr<OAuthIdentity> ExtractOAuthIdentity(ClaimsPrincipal claimsPrincipal);
}
