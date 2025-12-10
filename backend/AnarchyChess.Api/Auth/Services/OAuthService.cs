using AnarchyChess.Api.Auth.Errors;
using AnarchyChess.Api.Auth.Models;
using AnarchyChess.Api.Auth.OAuthAuthenticators;
using AnarchyChess.Api.Profile.Entities;
using AnarchyChess.Api.Profile.Services;
using AnarchyChess.Api.Shared.Services;
using ErrorOr;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using OpenIddict.Client.AspNetCore;

namespace AnarchyChess.Api.Auth.Services;

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
    IUsernameGenerator usernameGenerator,
    ICountryResolver countryResolver,
    IUnitOfWork unitOfWork
) : IOAuthService
{
    private readonly Dictionary<string, IOAuthAuthenticator> _authenticators =
        oauthAuthenticators.ToDictionary(x => x.Provider);

    private readonly UserManager<AuthedUser> _userManager = userManager;
    private readonly IAuthService _authService = authService;
    private readonly IUsernameGenerator _usernameGenerator = usernameGenerator;
    private readonly ICountryResolver _countryResolver = countryResolver;
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

        var oauthIdentityResult = oauthAuthenticator.ExtractOAuthIdentity(claimsPrincipal);
        if (oauthIdentityResult.IsError)
            return oauthIdentityResult.Errors;
        var oauthIdentity = oauthIdentityResult.Value;

        var userResult = await GetOrCreateUserAsync(oauthAuthenticator, oauthIdentity, context);
        if (userResult.IsError)
            return userResult.Errors;
        var user = userResult.Value;

        var tokens = await _authService.GenerateAuthTokensAsync(user, token);
        await _unitOfWork.CompleteAsync(token);
        return tokens;
    }

    private async Task<ErrorOr<AuthedUser>> GetOrCreateUserAsync(
        IOAuthAuthenticator authenticator,
        OAuthIdentity oauthIdentity,
        HttpContext context
    )
    {
        var existingLogin = await _userManager.FindByLoginAsync(
            authenticator.Provider,
            oauthIdentity.ProviderKey
        );
        if (existingLogin is not null)
            return existingLogin;

        var username = await _usernameGenerator.GenerateUniqueUsernameAsync();
        var countryCode = await _countryResolver.LocateAsync(
            context.Connection.RemoteIpAddress?.ToString()
        );
        var signupResult = await _authService.SignupAsync(
            username: username,
            email: oauthIdentity.Email,
            countryCode: countryCode
        );
        if (signupResult.IsError)
            return signupResult.Errors;
        var newUser = signupResult.Value;

        UserLoginInfo loginInfo = new(
            authenticator.Provider,
            oauthIdentity.ProviderKey,
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
