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
    IAuthService authService,
    IValidator<PaginationQuery> paginationValidator
) : ControllerBase
{
    private readonly IStarService _starService = starService;
    private readonly IAuthService _authService = authService;
    private readonly IValidator<PaginationQuery> _paginationValidator = paginationValidator;

    [HttpGet("stars/{userId}", Name = nameof(GetStars))]
    [ProducesResponseType<PagedResult<MinimalProfile>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResult<MinimalProfile>>> GetStars(
        string userId,
        [FromQuery] PaginationQuery pagination,
        CancellationToken token
    )
    {
        var validationResult = _paginationValidator.Validate(pagination);
        if (!validationResult.IsValid)
            return validationResult.Errors.ToErrorList().ToActionResult();

        var result = await _starService.GetStarsOfAsync(userId, pagination, token);
        return Ok(result);
    }

    [HttpGet("star/{starredUserId}/exists", Name = nameof(IsStarred))]
    [ProducesResponseType<bool>(StatusCodes.Status200OK)]
    [Authorize(AuthPolicies.AuthedUser)]
    public async Task<ActionResult<bool>> IsStarred(
        string starredUserId,
        CancellationToken token = default
    )
    {
        var userIdResult = _authService.GetUserId(User);
        if (userIdResult.IsError)
            return userIdResult.Errors.ToActionResult();

        var result = await _starService.IsStarredAsync(userIdResult.Value, starredUserId, token);
        return Ok(result);
    }

    [HttpPost("star/{starredUserId}", Name = nameof(AddStar))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ApiProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ApiProblemDetails>(StatusCodes.Status409Conflict)]
    [Authorize(AuthPolicies.AuthedUser)]
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
    [Authorize(AuthPolicies.AuthedUser)]
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
}
