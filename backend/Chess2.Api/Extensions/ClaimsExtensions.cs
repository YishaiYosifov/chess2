using ErrorOr;
using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;

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

    [return: NotNullIfNotNull(nameof(@default))]
    public static string? GetClaimValueOrDefault(
        this ClaimsPrincipal? claimsPrincipal,
        string claimType,
        string? @default = null
    )
    {
        if (claimsPrincipal is null)
            return @default;
        var claim = claimsPrincipal.Claims.FirstOrDefault(claim => claim.Type == claimType);
        if (claim is null)
            return @default;

        return claim.Value ?? @default;
    }
}
