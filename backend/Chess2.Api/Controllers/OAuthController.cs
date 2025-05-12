using Chess2.Api.Extensions;
using Chess2.Api.Services.Auth;
using Microsoft.AspNetCore.Mvc;
using static OpenIddict.Client.WebIntegration.OpenIddictClientWebIntegrationConstants;

namespace Chess2.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OAuthController(IOAuthService oAuthService) : Controller
{
    private readonly IOAuthService _oAuthService = oAuthService;

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
        return Challenge(Providers.Discord);
    }
}
