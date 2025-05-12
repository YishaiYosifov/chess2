using Chess2.Api.Errors;
using Chess2.Api.Models.DTOs;
using Chess2.Api.Services.Auth.OAuthAuthenticators;
using ErrorOr;
using Microsoft.AspNetCore.Authentication;
using OpenIddict.Client.AspNetCore;

namespace Chess2.Api.Services.Auth;

public interface IOAuthService
{
    Task<ErrorOr<Tokens>> AuthenticateAsync(string provider, HttpContext context);
}

public class OAuthService(
    IEnumerable<IOAuthAuthenticator> oauthAuthenticators,
    IAuthService authService
) : IOAuthService
{
    private readonly Dictionary<string, IOAuthAuthenticator> _oauthAuthenticators =
        oauthAuthenticators.ToDictionary(x => x.Provider, x => x);

    private readonly IAuthService _authService = authService;

    public async Task<ErrorOr<Tokens>> AuthenticateAsync(string provider, HttpContext context)
    {
        var result = await context.AuthenticateAsync(
            OpenIddictClientAspNetCoreDefaults.AuthenticationScheme
        );
        if (!result.Succeeded)
            return AuthErrors.OAuthInvalid;

        var claimsPrincipal = result.Principal;
        var oauthAuthenticatorResult = GetOAuthAuthenticator(provider);
        if (oauthAuthenticatorResult.IsError)
            return oauthAuthenticatorResult.Errors;
        var oauthAuthenticator = oauthAuthenticatorResult.Value;

        var userResult = await oauthAuthenticator.AuthenticateAsync(claimsPrincipal);
        if (userResult.IsError)
            return userResult.Errors;
        var user = userResult.Value;

        var signinResult = _authService.Signin(user, context);
        if (signinResult.IsError)
            return signinResult.Errors;
        var tokens = signinResult.Value;

        return tokens;
    }

    private ErrorOr<IOAuthAuthenticator> GetOAuthAuthenticator(string provider) =>
        _oauthAuthenticators.TryGetValue(provider, out var authenticator)
            ? ErrorOrFactory.From(authenticator)
            : AuthErrors.OAuthProviderNotFound;
}
