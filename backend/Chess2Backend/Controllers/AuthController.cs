using Chess2Backend.Models;
using Chess2Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace Chess2Backend.Controllers;

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
