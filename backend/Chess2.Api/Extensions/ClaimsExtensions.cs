using System.Security.Claims;
using ErrorOr;

namespace Chess2.Api.Extensions;

public static class ClaimsExtensions
{
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
