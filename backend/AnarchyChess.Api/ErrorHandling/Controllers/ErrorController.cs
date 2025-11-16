using AnarchyChess.Api.Shared.Models;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace AnarchyChess.Api.ErrorHandling.Controllers;

[Route("/error")]
public class ErrorController(ILogger<ErrorController> logger, IOptions<AppSettings> settings)
    : Controller
{
    private readonly ILogger<ErrorController> _logger = logger;
    private readonly AuthSettings _settings = settings.Value.Auth;

    public IActionResult Index()
    {
        var response = HttpContext.GetOpenIddictClientResponse();
        if (response is null)
            return StatusCode(HttpContext.Response.StatusCode);

        _logger.LogWarning("OpenIddict error occurred, {Error}", response);
        return Redirect(_settings.LoginPageUrl);
    }
}
