using System.Security.Claims;
using Chess2.Api.Errors;
using Chess2.Api.Extensions;
using Chess2.Api.Models.Entities;
using ErrorOr;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using static OpenIddict.Client.WebIntegration.OpenIddictClientWebIntegrationConstants;

namespace Chess2.Api.Services.Auth;

public interface IOAuthService
{
    Task<ErrorOr<AuthedUser>> AuthenticateGoogleAsync(HttpContext context);
}

public class OAuthService(
    LinkGenerator linkGenerator,
    SignInManager<AuthedUser> signinManager,
    UserManager<AuthedUser> userManager,
    IAuthService authService,
    IAuthCookieSetter authCookieSetter
) : IOAuthService
{
    private readonly SignInManager<AuthedUser> _signinManager = signinManager;
    private readonly IAuthCookieSetter _authCookieSetter = authCookieSetter;
    private readonly UserManager<AuthedUser> _userManager = userManager;
    private readonly IAuthService _authService = authService;

    public async Task<ErrorOr<AuthedUser>> AuthenticateGoogleAsync(HttpContext context)
    {
        //var result = await context.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);
        var result = await context.AuthenticateAsync(Providers.Discord);
        if (!result.Succeeded)
            return AuthErrors.OAuthInvalid;

        var claimsPrincipal = result.Principal;
        var userResult = await GetUserFromOAuthClaimsAsync(claimsPrincipal);
        if (userResult.IsError)
            return userResult.Errors;
        var user = userResult.Value;

        var loginInfo = new UserLoginInfo(
            "Google",
            claimsPrincipal.GetClaimValueOrDefault(ClaimTypes.NameIdentifier, string.Empty),
            "Google"
        );
        var signinResult = await _authService.SigninAsync(user, loginInfo);
        if (signinResult.IsError)
            return signinResult.Errors;
        var tokens = signinResult.Value;

        _authCookieSetter.SetAccessCookie(tokens.AccessToken, context);
        _authCookieSetter.SetRefreshCookie(tokens.RefreshToken, context);
        _authCookieSetter.SetIsAuthedCookie(context);

        return user;
    }

    private async Task<ErrorOr<AuthedUser>> GetUserFromOAuthClaimsAsync(
        ClaimsPrincipal? claimsPrincipal
    )
    {
        var claimEmailResult = claimsPrincipal.GetClaim(ClaimTypes.Email);
        if (claimEmailResult.IsError)
            return claimEmailResult.Errors;

        var emailClaim = claimEmailResult.Value;

        var user = await _userManager.FindByEmailAsync(emailClaim.Value);
        if (user is not null)
            return user;

        var signupResult = await _authService.SignupAsync(emailClaim.Value, emailClaim.Value);
        if (signupResult.IsError)
            return signupResult.Errors;
        user = signupResult.Value;
        return user;
    }
}
