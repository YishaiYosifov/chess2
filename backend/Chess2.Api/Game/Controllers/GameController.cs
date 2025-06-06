using Chess2.Api.Game.DTOs;
using Chess2.Api.Game.Services;
using Chess2.Api.Infrastructure.Errors;
using Chess2.Api.Infrastructure.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace Chess2.Api.Game.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GameController(IGameService gameService) : Controller
{
    private readonly IGameService _gameService = gameService;

    [HttpGet("/live/{gameToken}", Name = nameof(GetLiveGame))]
    [ProducesResponseType<GameStateDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task GetLiveGame(string gameToken, CancellationToken token)
    {
        var gameStateResult = await _gameService.GetGameStateAsync(gameToken, token);
        gameStateResult.Switch(value => Ok(value), errors => errors.ToActionResult());
    }
}
