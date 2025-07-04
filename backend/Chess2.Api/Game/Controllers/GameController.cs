using Akka.Hosting;
using Chess2.Api.Auth.Services;
using Chess2.Api.Game.Actors;
using Chess2.Api.Game.Models;
using Chess2.Api.Infrastructure;
using Chess2.Api.Infrastructure.Errors;
using Chess2.Api.Infrastructure.Extensions;
using Chess2.Api.Shared.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Chess2.Api.Game.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GameController(IRequiredActor<GameActor> gameActor, IAuthService authService)
    : Controller
{
    private readonly IRequiredActor<GameActor> _gameActor = gameActor;
    private readonly IAuthService _authService = authService;

    [HttpGet("live/{gameToken}", Name = nameof(GetLiveGame))]
    [ProducesResponseType<GameState>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiProblemDetails>(StatusCodes.Status404NotFound)]
    [Authorize(AuthPolicies.AuthedSesssion)]
    public async Task<ActionResult<GameState>> GetLiveGame(
        string gameToken,
        CancellationToken token
    )
    {
        var userIdResult = _authService.GetUserId(User);
        if (userIdResult.IsError)
            return userIdResult.Errors.ToActionResult();

        var gameStateResult = await _gameActor.ActorRef.AskExpecting<GameEvents.GameStateEvent>(
            new GameQueries.GetGameState(gameToken, userIdResult.Value),
            token
        );
        return gameStateResult.Match(value => Ok(value.State), errors => errors.ToActionResult());
    }
}
