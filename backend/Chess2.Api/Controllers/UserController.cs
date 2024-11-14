using Chess2.Api.Extensions;
using Chess2.Api.Models.DTOs;
using Chess2.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Chess2.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController(IAuthService authService) : ControllerBase
{
    private readonly IAuthService _authService = authService;

    [HttpGet("authed")]
    [ProducesResponseType<PrivateUserOut>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [Authorize]
    public async Task<IResult> GetAuthedUser(CancellationToken cancellation)
    {
        var result = await _authService.GetLoggedInUser(HttpContext, cancellation);
        return result.Match(
            (value) => Results.Ok(new PrivateUserOut(value)),
            (errors) => errors.ToProblemDetails());
    }
}
