using Chess2.Api.Auth.Services;
using Chess2.Api.Challenges.Grains;
using Chess2.Api.Challenges.Models;
using Chess2.Api.Infrastructure.Errors;
using Chess2.Api.Infrastructure.Extensions;
using Chess2.Api.Matchmaking.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Chess2.Api.Challenges.Controllers;

[ApiController]
[Route("/api/[controller]")]
[Authorize]
public class ChallengeController(IGrainFactory grains, IAuthService authService) : Controller
{
    private readonly IGrainFactory _grains = grains;
    private readonly IAuthService _authService = authService;

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

        var id = Guid.NewGuid().ToString()[..16];
        var challengeGrain = _grains.GetGrain<IChallengeGrain>(id);
        var result = await challengeGrain.CreateAsync(
            requester: userIdResult.Value,
            recipient: recipientId,
            pool
        );
        return result.Match(Ok, errors => errors.ToActionResult());
    }

    [HttpGet("{challengeId}", Name = nameof(GetChallenge))]
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

    [HttpDelete("{challengeId}", Name = nameof(CancelChallenge))]
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

    [HttpPost("{challengeId}/accept")]
    [ProducesResponseType<string>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiProblemDetails>(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<string>> AcceptChallenge(string challengeId)
    {
        var userIdResult = _authService.GetUserId(User);
        if (userIdResult.IsError)
            return userIdResult.Errors.ToActionResult();

        var challengeGrain = _grains.GetGrain<IChallengeGrain>(challengeId);
        var result = await challengeGrain.AcceptAsync(userIdResult.Value);
        return result.Match(Ok, errors => errors.ToActionResult());
    }
}
