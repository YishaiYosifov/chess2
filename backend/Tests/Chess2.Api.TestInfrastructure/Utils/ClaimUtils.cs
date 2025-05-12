using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using OpenIddict.Client.AspNetCore;

namespace Chess2.Api.TestInfrastructure.Utils;

public static class ClaimUtils
{
    public static AuthenticationTicket CreateAuthenticationTicket(Claim claim)
    {
        var identity = new ClaimsIdentity([claim], "Test");
        var claimsPrincipal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(
            claimsPrincipal,
            OpenIddictClientAspNetCoreDefaults.AuthenticationScheme
        );
        return ticket;
    }
}
