using Chess2.Api.Auth.Services;
using Chess2.Api.Challenges.Grains;
using Chess2.Api.Challenges.Models;
using Chess2.Api.Challenges.Services;
using Chess2.Api.GameSnapshot.Models;
using Chess2.Api.Infrastructure;
using Chess2.Api.Infrastructure.Errors;
using Chess2.Api.Infrastructure.Extensions;
using Chess2.Api.Matchmaking.Models;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Chess2.Api.Challenges.Controllers;

[ApiController]
[Route("/api/[controller]")]
[Authorize(AuthPolicies.ActiveSession)]
public class ChallengeController(
    IGrainFactory grains,
    IChallengeRequestCreator challengeRequestCreator,
    IAuthService authService,
    IValidator<TimeControlSettings> timeControlValidator
) : Controller
{
    private readonly IGrainFactory _grains = grains;
    private readonly IChallengeRequestCreator _challengeRequestCreator = challengeRequestCreator;
    private readonly IAuthService _authService = authService;
    private readonly IValidator<TimeControlSettings> _timeControlValidator = timeControlValidator;

    [HttpPut]
    [ProducesResponseType<ChallengeRequest>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ApiProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ApiProblemDetails>(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ChallengeRequest>> CreateChallenge(
        [FromQuery] string? recipientId,
        PoolKey pool
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
            pool
        );
        if (challengeRequestResult.IsError)
            return challengeRequestResult.Errors.ToActionResult();
        var challengeRequest = challengeRequestResult.Value;

        var challengeGrain = _grains.GetGrain<IChallengeGrain>(challengeRequest.ChallengeId);
        await challengeGrain.CreateAsync(challengeRequest);
        return Ok(challengeRequest);
    }

    [HttpGet("by-id/{challengeId}")]
    [ProducesResponseType<ChallengeRequest>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ChallengeRequest>> GetChallenge(string challengeId)
    {
        var userIdResult = _authService.GetUserId(User);
        if (userIdResult.IsError)
            return userIdResult.Errors.ToActionResult();

        var challengeGrain = _grains.GetGrain<IChallengeGrain>(challengeId);
        var result = await challengeGrain.GetAsync(requestedBy: userIdResult.Value);
        return result.Match(Ok, errors => errors.ToActionResult());
    }

    [HttpDelete("by-id/{challengeId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ApiProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> CancelChallenge(string challengeId)
    {
        var userIdResult = _authService.GetUserId(User);
        if (userIdResult.IsError)
            return userIdResult.Errors.ToActionResult();

        var challengeGrain = _grains.GetGrain<IChallengeGrain>(challengeId);
        var result = await challengeGrain.CancelAsync(userIdResult.Value);
        return result.Match(value => NoContent(), errors => errors.ToActionResult());
    }

    [HttpPost("by-id/{challengeId}/accept")]
    [ProducesResponseType<string>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiProblemDetails>(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<string>> AcceptChallenge(string challengeId)
    {
        var userIdResult = _authService.GetUserId(User);
        if (userIdResult.IsError)
            return userIdResult.Errors.ToActionResult();

        var challengeGrain = _grains.GetGrain<IChallengeGrain>(challengeId);
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
            var challengeGrain = _grains.GetGrain<IChallengeGrain>(challenge.ChallengeId);
            await challengeGrain.CancelAsync(userId);
        }

        return NoContent();
    }
}
