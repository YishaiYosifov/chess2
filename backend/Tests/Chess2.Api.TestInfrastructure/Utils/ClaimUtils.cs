using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using OpenIddict.Client.AspNetCore;

namespace Chess2.Api.TestInfrastructure.Utils;

public static class ClaimUtils
{
    public static AuthenticationTicket CreateAuthenticationTicket(Claim claim)
    {
        var principal = CreateClaimsPrincipal(claim);
        var ticket = new AuthenticationTicket(
            principal,
            OpenIddictClientAspNetCoreDefaults.AuthenticationScheme
        );
        return ticket;
    }

    public static ClaimsPrincipal CreateClaimsPrincipal(params IEnumerable<Claim> claims)
    {
        var identity = new ClaimsIdentity(claims);
        var principal = new ClaimsPrincipal(identity);
        return principal;
    }

    public static ClaimsPrincipal CreateUserClaims(int userId, params IEnumerable<Claim> claims) =>
        CreateClaimsPrincipal(
            claims.Append(new Claim(ClaimTypes.NameIdentifier, userId.ToString()))
        );
}
