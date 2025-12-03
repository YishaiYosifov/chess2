using AnarchyChess.Api.Auth.Services;
using AnarchyChess.Api.Challenges.Grains;
using AnarchyChess.Api.Challenges.Models;
using AnarchyChess.Api.Challenges.Services;
using AnarchyChess.Api.ErrorHandling.Extensions;
using AnarchyChess.Api.ErrorHandling.Infrastructure;
using AnarchyChess.Api.GameSnapshot.Models;
using AnarchyChess.Api.Infrastructure;
using AnarchyChess.Api.Infrastructure.Extensions;
using AnarchyChess.Api.Matchmaking.Models;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AnarchyChess.Api.Challenges.Controllers;

[ApiController]
[Route("/api/[controller]")]
[Authorize(AuthPolicies.ActiveSession)]
public class ChallengeController(
    IGrainFactory grains,
    IChallengeRequestCreator challengeRequestCreator,
    IAuthService authService,
    IValidator<TimeControlSettingsRequest> timeControlValidator
) : Controller
{
    private readonly IGrainFactory _grains = grains;
    private readonly IChallengeRequestCreator _challengeRequestCreator = challengeRequestCreator;
    private readonly IAuthService _authService = authService;
    private readonly IValidator<TimeControlSettingsRequest> _timeControlValidator =
        timeControlValidator;

    [HttpPut]
    [ProducesResponseType<ChallengeRequest>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ApiProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ApiProblemDetails>(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ChallengeRequest>> CreateChallenge(
        [FromQuery] string? recipientId,
        PoolKeyRequest pool
    )
    {
        var validationResult = _timeControlValidator.Validate(pool.TimeControl);
        if (!validationResult.IsValid)
            return validationResult.Errors.ToErrorList().ToActionResult();

        var userIdResult = _authService.GetUserId(User);
        if (userIdResult.IsError)
            return userIdResult.Errors.ToActionResult();

        var challengeRequestResult = await _challengeRequestCreator.CreateAsync(
            requesterId: userIdResult.Value,
            recipientId: recipientId,
            new PoolKey(pool)
        );
        if (challengeRequestResult.IsError)
            return challengeRequestResult.Errors.ToActionResult();
        var challengeRequest = challengeRequestResult.Value;

        var challengeGrain = _grains.GetGrain<IChallengeGrain>(challengeRequest.ChallengeToken);
        await challengeGrain.CreateAsync(challengeRequest);
        return Ok(challengeRequest);
    }

    [HttpGet("by-id/{challengeToken}")]
    [ProducesResponseType<ChallengeRequest>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ChallengeRequest>> GetChallenge(string challengeToken)
    {
        var userIdResult = _authService.GetUserId(User);
        if (userIdResult.IsError)
            return userIdResult.Errors.ToActionResult();

        var challengeGrain = _grains.GetGrain<IChallengeGrain>(challengeToken);
        var result = await challengeGrain.GetAsync(requestedBy: userIdResult.Value);
        return result.Match(Ok, errors => errors.ToActionResult());
    }

    [HttpDelete("by-id/{challengeToken}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ApiProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> CancelChallenge(string challengeToken)
    {
        var userIdResult = _authService.GetUserId(User);
        if (userIdResult.IsError)
            return userIdResult.Errors.ToActionResult();

        var challengeGrain = _grains.GetGrain<IChallengeGrain>(challengeToken);
        var result = await challengeGrain.CancelAsync(userIdResult.Value);
        return result.Match(value => NoContent(), errors => errors.ToActionResult());
    }

    [HttpPost("by-id/{challengeToken}/accept")]
    [ProducesResponseType<string>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiProblemDetails>(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<string>> AcceptChallenge(string challengeToken)
    {
        var userIdResult = _authService.GetUserId(User);
        if (userIdResult.IsError)
            return userIdResult.Errors.ToActionResult();

        var challengeGrain = _grains.GetGrain<IChallengeGrain>(challengeToken);
        var result = await challengeGrain.AcceptAsync(userIdResult.Value);
        return result.Match(value => Ok(value.Value), errors => errors.ToActionResult());
    }

    [HttpDelete("incoming")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [Authorize(AuthPolicies.AuthedUser)]
    public async Task<ActionResult> CancelAllIncomingChallenges()
    {
        var userIdResult = _authService.GetUserId(User);
        if (userIdResult.IsError)
            return userIdResult.Errors.ToActionResult();
        var userId = userIdResult.Value;

        var inboxGrain = _grains.GetGrain<IChallengeInboxGrain>(userId);
        var challenges = await inboxGrain.GetIncomingChallengesAsync();
        foreach (var challenge in challenges)
        {
            var challengeGrain = _grains.GetGrain<IChallengeGrain>(challenge.ChallengeToken);
            await challengeGrain.CancelAsync(userId);
        }

        return NoContent();
    }
}
