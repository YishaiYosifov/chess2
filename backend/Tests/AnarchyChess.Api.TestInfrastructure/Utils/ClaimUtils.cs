using System.Security.Claims;
using AnarchyChess.Api.Profile.Models;
using Microsoft.AspNetCore.Authentication;
using OpenIddict.Client.AspNetCore;

namespace AnarchyChess.Api.TestInfrastructure.Utils;

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

    public static ClaimsPrincipal CreateUserClaims(
        UserId userId,
        params IEnumerable<Claim> claims
    ) => CreateClaimsPrincipal(claims.Append(new Claim(ClaimTypes.NameIdentifier, userId)));

    public static ClaimsPrincipal CreateGuestClaims(
        UserId userId,
        params IEnumerable<Claim> claims
    ) => CreateUserClaims(userId, claims.Append(new Claim(ClaimTypes.Anonymous, "1")));
}
