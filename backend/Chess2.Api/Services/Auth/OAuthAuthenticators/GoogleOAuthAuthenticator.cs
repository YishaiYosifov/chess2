using System.Security.Claims;
using Chess2.Api.Extensions;
using Chess2.Api.Models.Entities;
using ErrorOr;
using Microsoft.AspNetCore.Identity;
using static OpenIddict.Client.WebIntegration.OpenIddictClientWebIntegrationConstants;

namespace Chess2.Api.Services.Auth.OAuthAuthenticators;

public class GoogleOAuthAuthenticator(UserManager<AuthedUser> userManager, IAuthService authService)
    : IOAuthAuthenticator
{
    private readonly UserManager<AuthedUser> _userManager = userManager;
    private readonly IAuthService _authService = authService;

    public string Provider => Providers.Google;

    public async Task<ErrorOr<AuthedUser>> AuthenticateAsync(ClaimsPrincipal claimsPrincipal)
    {
        var claimEmailResult = claimsPrincipal.GetClaim(ClaimTypes.Email);
        if (claimEmailResult.IsError)
            return claimEmailResult.Errors;
        var emailClaim = claimEmailResult.Value;

        var existingLogin = await _userManager.FindByLoginAsync(Provider, emailClaim.Value);
        if (existingLogin is not null)
            return existingLogin;

        var signupResult = await _authService.SignupAsync(emailClaim.Value, emailClaim.Value);
        if (signupResult.IsError)
            return signupResult.Errors;
        var user = signupResult.Value;

        await _userManager.AddLoginAsync(
            user,
            new UserLoginInfo(Provider, emailClaim.Value, Provider)
        );

        return user;
    }
}
