using Chess2.Api.Errors;
using Chess2.Api.Models.Requests;
using Chess2.Api.Repositories;
using Chess2.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Chess2.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(ILogger<AuthController> logger, TokenService tokenService, IUserRepository userRepository) : Controller
{
    private readonly IUserRepository _userRepository = userRepository;
    private readonly TokenService _tokenService = tokenService;
    private readonly ILogger<AuthController> _logger = logger;

    [HttpPost("register")]
    public async Task<IResult> Register([FromBody] UserIn userIn, CancellationToken cancellation)
    {
        var result = await _userRepository.RegisterUser(userIn, cancellation);
        if (result.IsSuccess)
            _logger.LogInformation("Created user {Username}", userIn.Username);
        return result.ToResult();
    }
}
