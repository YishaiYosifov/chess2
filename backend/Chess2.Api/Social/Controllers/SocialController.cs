using Chess2.Api.Auth.Services;
using Chess2.Api.Infrastructure;
using Chess2.Api.Infrastructure.Errors;
using Chess2.Api.Infrastructure.Extensions;
using Chess2.Api.Pagination.Models;
using Chess2.Api.Profile.DTOs;
using Chess2.Api.Shared.Models;
using Chess2.Api.Social.Services;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Chess2.Api.Social.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SocialController(
    IStarService starService,
    IBlockService blockService,
    IAuthService authService,
    IValidator<PaginationQuery> paginationValidator
) : ControllerBase
{
    private readonly IStarService _starService = starService;
    private readonly IBlockService _blockService = blockService;
    private readonly IAuthService _authService = authService;
    private readonly IValidator<PaginationQuery> _paginationValidator = paginationValidator;

    [HttpGet("starred/{userId}", Name = nameof(GetStarredUsers))]
    [ProducesResponseType<PagedResult<MinimalProfile>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResult<MinimalProfile>>> GetStarredUsers(
        string userId,
        [FromQuery] PaginationQuery pagination,
        CancellationToken token
    )
    {
        var validationResult = _paginationValidator.Validate(pagination);
        if (!validationResult.IsValid)
            return validationResult.Errors.ToErrorList().ToActionResult();

        var result = await _starService.GetStarredUsersAsync(userId, pagination, token);
        return Ok(result);
    }

    [HttpGet("stars/{starredUserId}", Name = nameof(GetStarsReceivedCount))]
    [ProducesResponseType<int>(StatusCodes.Status200OK)]
    public async Task<ActionResult<int>> GetStarsReceivedCount(
        string starredUserId,
        CancellationToken token
    )
    {
        var count = await _starService.GetStarsReceivedCountAsync(starredUserId, token);
        return Ok(count);
    }

    [HttpGet("star/{starredUserId}/exists", Name = nameof(GetHasStarred))]
    [ProducesResponseType<bool>(StatusCodes.Status200OK)]
    [Authorize(AuthPolicies.AuthedUser)]
    public async Task<ActionResult<bool>> GetHasStarred(
        string starredUserId,
        CancellationToken token = default
    )
    {
        var userIdResult = _authService.GetUserId(User);
        if (userIdResult.IsError)
            return userIdResult.Errors.ToActionResult();

        var result = await _starService.HasStarredAsync(userIdResult.Value, starredUserId, token);
        return Ok(result);
    }

    [HttpPost("star/{starredUserId}", Name = nameof(AddStar))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ApiProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ApiProblemDetails>(StatusCodes.Status409Conflict)]
    [Authorize]
    public async Task<ActionResult> AddStar(string starredUserId, CancellationToken token)
    {
        var userIdResult = _authService.GetUserId(User);
        if (userIdResult.IsError)
            return userIdResult.Errors.ToActionResult();

        var result = await _starService.AddStarAsync(
            forUserId: userIdResult.Value,
            starredUserId,
            token
        );
        return result.Match(value => NoContent(), errors => errors.ToActionResult());
    }

    [HttpDelete("star/{starredUserId}", Name = nameof(RemoveStar))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Authorize]
    public async Task<ActionResult> RemoveStar(string starredUserId, CancellationToken token)
    {
        var userIdResult = _authService.GetUserId(User);
        if (userIdResult.IsError)
            return userIdResult.Errors.ToActionResult();

        var result = await _starService.RemoveStarAsync(
            forUserId: userIdResult.Value,
            starredUserId,
            token
        );
        return result.Match(value => NoContent(), errors => errors.ToActionResult());
    }

    [HttpGet("blocked", Name = nameof(GetBlockedUsers))]
    [ProducesResponseType<PagedResult<MinimalProfile>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiProblemDetails>(StatusCodes.Status400BadRequest)]
    [Authorize]
    public async Task<ActionResult<PagedResult<MinimalProfile>>> GetBlockedUsers(
        [FromQuery] PaginationQuery pagination,
        CancellationToken token = default
    )
    {
        var validationResult = _paginationValidator.Validate(pagination);
        if (!validationResult.IsValid)
            return validationResult.Errors.ToErrorList().ToActionResult();

        var userIdResult = _authService.GetUserId(User);
        if (userIdResult.IsError)
            return userIdResult.Errors.ToActionResult();

        var result = await _blockService.GetBlockedUsersAsync(
            userIdResult.Value,
            pagination,
            token
        );
        return Ok(result);
    }

    [HttpGet("block/{blockedUserId}/exists", Name = nameof(GetHasBlocked))]
    [ProducesResponseType<bool>(StatusCodes.Status200OK)]
    [Authorize]
    public async Task<ActionResult<bool>> GetHasBlocked(
        string blockedUserId,
        CancellationToken token = default
    )
    {
        var userIdResult = _authService.GetUserId(User);
        if (userIdResult.IsError)
            return userIdResult.Errors.ToActionResult();

        var result = await _blockService.HasBlockedAsync(userIdResult.Value, blockedUserId, token);
        return Ok(result);
    }

    [HttpPost("block/{blockedUserId}", Name = nameof(BlockUser))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ApiProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ApiProblemDetails>(StatusCodes.Status409Conflict)]
    [Authorize]
    public async Task<ActionResult> BlockUser(string blockedUserId, CancellationToken token)
    {
        var userIdResult = _authService.GetUserId(User);
        if (userIdResult.IsError)
            return userIdResult.Errors.ToActionResult();

        var result = await _blockService.BlockUserAsync(userIdResult.Value, blockedUserId, token);
        return result.Match(value => NoContent(), errors => errors.ToActionResult());
    }

    [HttpDelete("block/{blockedUserId}", Name = nameof(UnblockUser))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Authorize]
    public async Task<ActionResult> UnblockUser(string blockedUserId, CancellationToken token)
    {
        var userIdResult = _authService.GetUserId(User);
        if (userIdResult.IsError)
            return userIdResult.Errors.ToActionResult();

        var result = await _blockService.UnblockUserAsync(userIdResult.Value, blockedUserId, token);
        return result.Match(value => NoContent(), errors => errors.ToActionResult());
    }
}
