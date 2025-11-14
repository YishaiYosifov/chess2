using AnarchyChess.Api.ArchivedGames.Models;
using AnarchyChess.Api.ArchivedGames.Services;
using AnarchyChess.Api.Auth.Services;
using AnarchyChess.Api.GameSnapshot.Models;
using AnarchyChess.Api.Infrastructure;
using AnarchyChess.Api.Infrastructure.Errors;
using AnarchyChess.Api.Infrastructure.Extensions;
using AnarchyChess.Api.Game.Grains;
using AnarchyChess.Api.Pagination.Models;
using AnarchyChess.Api.Shared.Models;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AnarchyChess.Api.Game.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GameController(
    IGameArchiveService gameArchiveService,
    IAuthService authService,
    IValidator<PaginationQuery> paginationValidator,
    IGrainFactory grains
) : Controller
{
    private readonly IGameArchiveService _gameArchiveService = gameArchiveService;
    private readonly IAuthService _authService = authService;
    private readonly IValidator<PaginationQuery> _paginationValidator = paginationValidator;
    private readonly IGrainFactory _grains = grains;

    [HttpGet("{gameToken}")]
    [ProducesResponseType<GameState>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiProblemDetails>(StatusCodes.Status404NotFound)]
    [Authorize(AuthPolicies.ActiveSession)]
    public async Task<ActionResult<GameState>> GetGame(string gameToken)
    {
        var userIdResult = _authService.GetUserId(User);
        if (userIdResult.IsError)
            return userIdResult.Errors.ToActionResult();

        var gameGrain = _grains.GetGrain<IGameGrain>(gameToken);
        var result = await gameGrain.GetStateAsync(userIdResult.Value);
        return result.Match(Ok, errors => errors.ToActionResult());
    }

    [HttpGet("results/{userId}")]
    [ProducesResponseType<PagedResult<GameSummaryDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<GameSummaryDto>>> GetGameResults(
        string userId,
        [FromQuery] PaginationQuery pagination,
        CancellationToken token
    )
    {
        var validationResult = _paginationValidator.Validate(pagination);
        if (!validationResult.IsValid)
            return validationResult.Errors.ToErrorList().ToActionResult();

        var result = await _gameArchiveService.GetPaginatedResultsAsync(userId, pagination, token);
        return Ok(result);
    }
}
