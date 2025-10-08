using Chess2.Api.Auth.Services;
using Chess2.Api.Challenges.Grains;
using Chess2.Api.Challenges.Models;
using Chess2.Api.Challenges.Services;
using Chess2.Api.Infrastructure;
using Chess2.Api.Infrastructure.Errors;
using Chess2.Api.Infrastructure.Extensions;
using Chess2.Api.Matchmaking.Models;
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
    IGuestService guestService
) : Controller
{
    private readonly IGrainFactory _grains = grains;
    private readonly IChallengeRequestCreator _challengeRequestCreator = challengeRequestCreator;
    private readonly IAuthService _authService = authService;
    private readonly IGuestService _guestService = guestService;

    [HttpPut(Name = nameof(CreateChallenge))]
    [ProducesResponseType<ChallengeRequest>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ApiProblemDetails>(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ChallengeRequest>> CreateChallenge(
        [FromQuery] string? recipientId,
        PoolKey pool
    )
    {
        var userIdResult = _authService.GetUserId(User);
        if (userIdResult.IsError)
            return userIdResult.Errors.ToActionResult();

        var challengeRequestResult = await _challengeRequestCreator.CreateAsync(
            requesterId: userIdResult.Value,
            isGuest: _guestService.IsGuest(User),
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

    [HttpGet("by-id/{challengeId}", Name = nameof(GetChallenge))]
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

    [HttpDelete("by-id/{challengeId}", Name = nameof(CancelChallenge))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ApiProblemDetails>(StatusCodes.Status404NotFound)]
    [Authorize(AuthPolicies.AuthedUser)]
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
        bool isGuest = _guestService.IsGuest(User);

        var challengeGrain = _grains.GetGrain<IChallengeGrain>(challengeId);
        var result = await challengeGrain.AcceptAsync(userIdResult.Value, isGuest);
        return result.Match(Ok, errors => errors.ToActionResult());
    }
}
