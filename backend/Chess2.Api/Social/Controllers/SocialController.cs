using Chess2.Api.Auth.Services;
using Chess2.Api.Infrastructure;
using Chess2.Api.Infrastructure.Errors;
using Chess2.Api.Infrastructure.Extensions;
using Chess2.Api.Pagination.Models;
using Chess2.Api.Profile.DTOs;
using Chess2.Api.Profile.Entities;
using Chess2.Api.Profile.Errors;
using Chess2.Api.Shared.Models;
using Chess2.Api.Social.Services;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Chess2.Api.Social.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SocialController(
    IFriendService friendService,
    IAuthService authService,
    UserManager<AuthedUser> userManager,
    IValidator<PaginationQuery> paginationValidator
) : ControllerBase
{
    private readonly IFriendService _friendService = friendService;
    private readonly IAuthService _authService = authService;
    private readonly UserManager<AuthedUser> _userManager = userManager;
    private readonly IValidator<PaginationQuery> _paginationValidator = paginationValidator;

    [HttpGet("friends", Name = nameof(GetFriends))]
    [ProducesResponseType<PagedResult<MinimalProfile>>(StatusCodes.Status200OK)]
    [Authorize(AuthPolicies.AuthedUser)]
    public async Task<ActionResult<PagedResult<MinimalProfile>>> GetFriends(
        [FromQuery] PaginationQuery pagination,
        CancellationToken token
    )
    {
        var validationResult = _paginationValidator.Validate(pagination);
        if (!validationResult.IsValid)
            return validationResult.Errors.ToErrorList().ToActionResult();

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

    [HttpPost("friends/request/{userId}", Name = nameof(RequestFriend))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ApiProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ApiProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ApiProblemDetails>(StatusCodes.Status409Conflict)]
    [Authorize(AuthPolicies.AuthedUser)]
    public async Task<ActionResult> RequestFriend(string userId, CancellationToken token)
    {
        var loggedInUserResult = await _authService.GetLoggedInUserAsync(User);
        if (loggedInUserResult.IsError)
            return loggedInUserResult.Errors.ToActionResult();

        var recipient = await _userManager.FindByIdAsync(userId);
        if (recipient is null)
            return ProfileErrors.NotFound.ToActionResult();

        var result = await _friendService.RequestFriendAsync(
            requester: loggedInUserResult.Value,
            recipient,
            token
        );
        return result.Match(value => NoContent(), errors => errors.ToActionResult());
    }
}
