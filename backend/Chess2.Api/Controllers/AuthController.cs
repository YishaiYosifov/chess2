using Chess2.Api.Models;
using Chess2.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Chess2.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthController(TokenService tokenService, ILogger<AuthController> logger) : Controller
{
    private readonly TokenService _tokenService = tokenService;
    private readonly ILogger<AuthController> _logger = logger;

    [HttpPost("register")]
    public void Register([FromBody] UserLogin userLogin)
    {
        _logger.LogInformation(userLogin.Username);
    }
}
