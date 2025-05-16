using Chess2.Api.Extensions;
using Chess2.Api.Models;
using Chess2.Api.Services.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Chess2.Api.Controllers;

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
    private readonly AppSettings _settings = settings.Value;

    [HttpGet("{provider}/callback", Name = nameof(OAuthCallback))]
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
                _authCookieSetter.SetCookies(value.AccessToken, value.RefreshToken, HttpContext);
                return Redirect(_settings.OAuthRedirectUrl);
            },
            errors => errors.ToActionResult()
        );
    }

    [HttpGet("signin/{provider}", Name = nameof(SigninOAuth))]
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
