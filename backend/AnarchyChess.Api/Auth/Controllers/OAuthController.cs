using AnarchyChess.Api.Auth.Services;
using AnarchyChess.Api.ErrorHandling.Extensions;
using AnarchyChess.Api.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace AnarchyChess.Api.Auth.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OAuthController(
    IOAuthService oAuthService,
    IAuthCookieSetter authCookieSetter,
    IOAuthProviderNameNormalizer oAuthProviderNameNormalizer,
    IOptions<AppSettings> settings
) : Controller
{
    private readonly IOAuthService _oAuthService = oAuthService;
    private readonly IAuthCookieSetter _authCookieSetter = authCookieSetter;
    private readonly IOAuthProviderNameNormalizer _oAuthProviderNameNormalizer =
        oAuthProviderNameNormalizer;
    private readonly AuthSettings _settings = settings.Value.Auth;

    [HttpGet("{provider}/callback")]
    public async Task<ActionResult> OAuthCallback(
        string provider,
        CancellationToken token = default
    )
    {
        var normalizedProviderResult = _oAuthProviderNameNormalizer.NormalizeProviderName(provider);
        if (normalizedProviderResult.IsError)
            return normalizedProviderResult.Errors.ToActionResult();
        var normalizedProvider = normalizedProviderResult.Value;

        var authResults = await _oAuthService.AuthenticateAsync(
            normalizedProvider,
            HttpContext,
            token
        );
        return authResults.Match(
            value =>
            {
                _authCookieSetter.SetAuthCookies(
                    value.AccessToken,
                    value.RefreshToken,
                    HttpContext
                );
                return Redirect(_settings.OAuthRedirectUrl);
            },
            errors => errors.ToActionResult()
        );
    }

    [HttpGet("signin/{provider}")]
    [ProducesResponseType(StatusCodes.Status302Found)]
    public ActionResult SigninOAuth(string provider)
    {
        var normalizedProviderResult = _oAuthProviderNameNormalizer.NormalizeProviderName(provider);
        return normalizedProviderResult.Match(
            value => Challenge(value),
            error => error.ToActionResult()
        );
    }
}
