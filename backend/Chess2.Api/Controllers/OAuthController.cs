using Chess2.Api.Extensions;
using Chess2.Api.Services.Auth;
using Microsoft.AspNetCore.Mvc;

namespace Chess2.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OAuthController(IOAuthService oAuthService) : Controller
{
    private readonly IOAuthService _oAuthService = oAuthService;

    [HttpGet("google", Name = nameof(SigninGoogle))]
    [ProducesResponseType(StatusCodes.Status302Found)]
    public ActionResult SigninGoogle([FromQuery] string returnUrl)
    {
        var properties = _oAuthService.ConfigureGoogleOAuthProperties(returnUrl, HttpContext);
        return Challenge(properties, ["Google"]);
    }

    [HttpGet("google/callback", Name = nameof(SigninGoogleCallback))]
    public async Task<ActionResult> SigninGoogleCallback([FromQuery] string returnUrl)
    {
        var result = await _oAuthService.AuthenticateGoogleAsync(HttpContext);
        return result.Match(value => Redirect(returnUrl), errors => errors.ToActionResult());
    }
}
