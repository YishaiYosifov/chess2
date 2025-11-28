using AnarchyChess.Api.Auth.Errors;
using AnarchyChess.Api.Auth.Services;
using AnarchyChess.Api.Shared.Models;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace AnarchyChess.Api.ErrorHandling.Controllers;

[Route("/error")]
public class ErrorController(
    ILogger<ErrorController> logger,
    IOptions<AppSettings> settings,
    IAuthCookieSetter authCookieSetter
) : Controller
{
    private readonly ILogger<ErrorController> _logger = logger;
    private readonly IAuthCookieSetter _authCookieSetter = authCookieSetter;
    private readonly AuthSettings _settings = settings.Value.Auth;

    public IActionResult Index()
    {
        var response = HttpContext.GetOpenIddictClientResponse();
        if (response is null)
            return StatusCode(HttpContext.Response.StatusCode);

        _authCookieSetter.SetAuthFailureCookie(AuthErrors.OAuthFailure, HttpContext);
        _logger.LogWarning("OpenIddict error occurred, {Error}", response);
        return Redirect(_settings.LoginPageUrl);
    }
}
