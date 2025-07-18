using Chess2.Api.ArchivedGames.Models;
using Chess2.Api.ArchivedGames.Services;
using Chess2.Api.Auth.Services;
using Chess2.Api.GameSnapshot.Models;
using Chess2.Api.Infrastructure;
using Chess2.Api.Infrastructure.Errors;
using Chess2.Api.Infrastructure.Extensions;
using Chess2.Api.LiveGame.Services;
using Chess2.Api.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Chess2.Api.LiveGame.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GameController(
    ILiveGameService gameService,
    IGameArchiveService gameArchiveService,
    IAuthService authService
) : Controller
{
    private readonly ILiveGameService _gameService = gameService;
    private readonly IGameArchiveService _gameArchiveService = gameArchiveService;
    private readonly IAuthService _authService = authService;

    [HttpGet("{gameToken}", Name = nameof(GetGame))]
    [ProducesResponseType<GameState>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiProblemDetails>(StatusCodes.Status404NotFound)]
    [Authorize(AuthPolicies.AuthedSesssion)]
    public async Task<ActionResult<GameState>> GetGame(string gameToken, CancellationToken token)
    {
        var userIdResult = _authService.GetUserId(User);
        if (userIdResult.IsError)
            return userIdResult.Errors.ToActionResult();

        var gameStateResult = await _gameService.GetGameStateAsync(
            gameToken,
            userIdResult.Value,
            token
        );
        return gameStateResult.Match(Ok, errors => errors.ToActionResult());
    }

    [HttpGet("/results/{userId}", Name = nameof(GetGameResults))]
    [ProducesResponseType<PagedResult<GameSummaryDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<GameSummaryDto>>> GetGameResults(
        string userId,
        [FromQuery] PaginationQuery pagination,
        CancellationToken token
    )
    {
        var result = await _gameArchiveService.GetPaginatedResultsAsync(userId, pagination, token);
        return Ok(result);
    }
}
