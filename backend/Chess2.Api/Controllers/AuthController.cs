using Chess2.Api.Errors;
using Chess2.Api.Models.DTOs;
using Chess2.Api.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Chess2.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(ILogger<AuthController> logger, IUserRepository userRepository) : Controller
{
    private readonly IUserRepository _userRepository = userRepository;
    private readonly ILogger<AuthController> _logger = logger;

    [HttpPost("register")]
    [ProducesResponseType(typeof(UserOut), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ConflictError), StatusCodes.Status409Conflict)]
    public async Task<IResult> Register([FromBody] UserIn userIn, CancellationToken cancellation)
    {
        var result = await _userRepository.RegisterUser(userIn, cancellation);
        if (result.IsSuccess)
            _logger.LogInformation("Created user {Username}", userIn.Username);

        return result.Match(value =>
            Results.Ok(new UserOut(result.Value)));
    }
}
