using System.Security.Claims;
using Chess2.Api.Auth.DTOs;
using Chess2.Api.Auth.Errors;
using Chess2.Api.Auth.Services.OAuthAuthenticators;
using Chess2.Api.Shared.Services;
using Chess2.Api.Users.Entities;
using ErrorOr;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using OpenIddict.Client.AspNetCore;

namespace Chess2.Api.Auth.Services;

public interface IOAuthService
{
    Task<ErrorOr<Tokens>> AuthenticateAsync(
        string provider,
        HttpContext context,
        CancellationToken token = default
    );
}

public class OAuthService(
    IEnumerable<IOAuthAuthenticator> oauthAuthenticators,
    UserManager<AuthedUser> userManager,
    IAuthService authService,
    IUnitOfWork unitOfWork
) : IOAuthService
{
    private readonly Dictionary<string, IOAuthAuthenticator> _authenticators =
        oauthAuthenticators.ToDictionary(x => x.Provider, x => x);

    private readonly UserManager<AuthedUser> _userManager = userManager;
    private readonly IAuthService _authService = authService;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<ErrorOr<Tokens>> AuthenticateAsync(
        string provider,
        HttpContext context,
        CancellationToken token = default
    )
    {
        var result = await context.AuthenticateAsync(
            OpenIddictClientAspNetCoreDefaults.AuthenticationScheme
        );
        if (!result.Succeeded)
            return AuthErrors.OAuthInvalid;

        var claimsPrincipal = result.Principal;
        var oauthAuthenticatorResult = GetAuthenticator(provider);
        if (oauthAuthenticatorResult.IsError)
            return oauthAuthenticatorResult.Errors;
        var oauthAuthenticator = oauthAuthenticatorResult.Value;

        var providerKeyResult = oauthAuthenticator.GetProviderKey(claimsPrincipal);
        if (providerKeyResult.IsError)
            return providerKeyResult.Errors;
        var providerKey = providerKeyResult.Value;

        var userResult = await GetOrCreateUserAsync(
            oauthAuthenticator,
            claimsPrincipal,
            providerKey
        );
        if (userResult.IsError)
            return userResult.Errors;
        var user = userResult.Value;

        var tokens = await _authService.GenerateAuthTokensAsync(user, token);
        await _unitOfWork.CompleteAsync(token);
        return tokens;
    }

    private async Task<ErrorOr<AuthedUser>> GetOrCreateUserAsync(
        IOAuthAuthenticator authenticator,
        ClaimsPrincipal claimsPrincipal,
        string providerKey
    )
    {
        var existingLogin = await _userManager.FindByLoginAsync(
            authenticator.Provider,
            providerKey
        );
        if (existingLogin is not null)
            return existingLogin;

        var signupResult = await authenticator.SignUserUpAsync(claimsPrincipal, providerKey);
        if (signupResult.IsError)
            return signupResult.Errors;
        var newUser = signupResult.Value;

        var loginInfo = new UserLoginInfo(
            authenticator.Provider,
            providerKey,
            authenticator.Provider
        );
        await _userManager.AddLoginAsync(newUser, loginInfo);
        return signupResult;
    }

    private ErrorOr<IOAuthAuthenticator> GetAuthenticator(string provider) =>
        _authenticators.TryGetValue(provider, out var authenticator)
            ? ErrorOrFactory.From(authenticator)
            : AuthErrors.OAuthProviderNotFound;
}
