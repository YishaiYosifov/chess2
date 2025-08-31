using Chess2.Api.Auth.Services;
using Chess2.Api.Infrastructure;
using Chess2.Api.Infrastructure.Extensions;
using Chess2.Api.Pagination.Models;
using Chess2.Api.Profile.DTOs;
using Chess2.Api.Shared.Models;
using Chess2.Api.Social.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Chess2.Api.Social.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SocialController(IFriendService friendService, IAuthService authService)
    : ControllerBase
{
    private readonly IFriendService _friendService = friendService;
    private readonly IAuthService _authService = authService;

    [HttpGet("friends", Name = nameof(GetFriends))]
    [ProducesResponseType<PagedResult<MinimalProfile>>(StatusCodes.Status200OK)]
    [Authorize(AuthPolicies.AuthedUser)]
    public async Task<ActionResult<PagedResult<MinimalProfile>>> GetFriends(
        [FromQuery] PaginationQuery pagination,
        CancellationToken token
    )
    {
        var userIdResult = _authService.GetUserId(User);
        if (userIdResult.IsError)
            return userIdResult.Errors.ToActionResult();

        var result = await _friendService.GetFriendRequestsAsync(
            userIdResult.Value,
            pagination,
            token
        );
        return Ok(result);
    }
}
