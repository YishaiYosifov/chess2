using Chess2Backend.Models;
using Chess2Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace Chess2Backend.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthController(TokenService tokenService) : Controller
{
    private readonly TokenService _tokenService = tokenService;

    [HttpPost("Register")]
    public void Register([FromBody] UserLogin userLogin)
    {
        Console.WriteLine(userLogin);
    }
}
