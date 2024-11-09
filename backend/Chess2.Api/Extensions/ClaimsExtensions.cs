using ErrorOr;
using System.Diagnostics.CodeAnalysis;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Chess2.Api.Extensions;

public static class ClaimsExtensions
{
    [return: NotNull]
    public static ErrorOr<Claim> GetClaim(this ClaimsPrincipal? claimsPrincipal, string claimType)
    {
        if (claimsPrincipal is null)
            return Error.Unauthorized(description: "Could not find claim principal");

        var claim = claimsPrincipal.Claims.FirstOrDefault(claim => claim.Type == claimType);
        if (claim is null)
            return Error.Unauthorized(description: $"Could not find claim '{claimType}'");

        return claim;
    }
}
