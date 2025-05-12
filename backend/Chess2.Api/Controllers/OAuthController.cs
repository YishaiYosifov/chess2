using Chess2.Api.Extensions;
using Chess2.Api.Services.Auth;
using Microsoft.AspNetCore.Mvc;

namespace Chess2.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OAuthController(
    IOAuthService oAuthService,
    IOAuthProviderNameNormalizer oAuthProviderNameNormalizer
) : Controller
{
    private readonly IOAuthService _oAuthService = oAuthService;
    private readonly IOAuthProviderNameNormalizer _oAuthProviderNameNormalizer =
        oAuthProviderNameNormalizer;

    [HttpGet("{provider}/callback", Name = nameof(OAuthCallback))]
    public async Task<ActionResult> OAuthCallback(string provider)
    {
        var normalizedProviderResult = _oAuthProviderNameNormalizer.NormalizeProviderName(provider);
        if (normalizedProviderResult.IsError)
            return normalizedProviderResult.Errors.ToActionResult();
        var normalizedProvider = normalizedProviderResult.Value;

        var result = await _oAuthService.AuthenticateAsync(normalizedProvider, HttpContext);
        return result.Match(value => Redirect("/"), errors => errors.ToActionResult());
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
