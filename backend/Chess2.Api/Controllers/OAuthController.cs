using Chess2.Api.Extensions;
using Chess2.Api.Services.Auth;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Client.AspNetCore;
using OpenIddict.Client.WebIntegration;
using static OpenIddict.Client.WebIntegration.OpenIddictClientWebIntegrationConstants;

namespace Chess2.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OAuthController(IOAuthService oAuthService) : Controller
{
    private readonly IOAuthService _oAuthService = oAuthService;

    [HttpGet("google", Name = nameof(SigninGoogle))]
    [ProducesResponseType(StatusCodes.Status302Found)]
    public ActionResult SigninGoogle()
    {
        const string provider = OpenIddictClientWebIntegrationConstants.Providers.Google;
        var properties = _oAuthService.ConfigureOAuthProperties(
            provider,
            nameof(SigninGoogleCallback),
            HttpContext
        );
        return Challenge(properties, provider);
    }

    [HttpGet("google/callback", Name = nameof(SigninGoogleCallback))]
    public async Task<ActionResult> SigninGoogleCallback()
    {
        var result = await _oAuthService.AuthenticateGoogleAsync(HttpContext);
        return result.Match(value => Redirect("/"), errors => errors.ToActionResult());
    }

    [HttpGet("discord", Name = nameof(SigninDiscord))]
    [ProducesResponseType(StatusCodes.Status302Found)]
    public ActionResult SigninDiscord()
    {
        const string provider = Providers.Discord;
        var properties = _oAuthService.ConfigureOAuthProperties(
            provider,
            nameof(SigninGoogleCallback),
            HttpContext
        );
        return Challenge(properties, OpenIddictClientAspNetCoreDefaults.AuthenticationScheme);
    }
}
