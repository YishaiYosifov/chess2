using Chess2.Api.Auth.Services;
using Chess2.Api.Infrastructure.Extensions;
using Chess2.Api.Pagination.Models;
using Chess2.Api.Quests.DTOs;
using Chess2.Api.Shared.Models;
using Chess2.Api.Streaks.Models;
using Chess2.Api.Streaks.Services;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Chess2.Api.Streaks.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WinStreakController(
    IWinStreakService winStreakService,
    IAuthService authService,
    IValidator<PaginationQuery> paginationValidator
) : Controller
{
    private readonly IWinStreakService _winStreakService = winStreakService;
    private readonly IAuthService _authService = authService;
    private readonly IValidator<PaginationQuery> _paginationValidator = paginationValidator;

    [HttpGet("leaderboard")]
    [ProducesResponseType<PagedResult<QuestPointsDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<QuestPointsDto>>> GetWinStreakLeaderboard(
        [FromQuery] PaginationQuery pagination,
        CancellationToken token = default
    )
    {
        var validationResult = _paginationValidator.Validate(pagination);
        if (!validationResult.IsValid)
            return validationResult.Errors.ToErrorList().ToActionResult();

        var leaderboard = await _winStreakService.GetPaginatedLeaderboardAsync(pagination, token);
        return Ok(leaderboard);
    }

    [HttpGet("me")]
    [ProducesResponseType<MyWinStreakStats>(StatusCodes.Status200OK)]
    [Authorize]
    public async Task<ActionResult<MyWinStreakStats>> GetMyWinStreakStats(
        CancellationToken token = default
    )
    {
        var userIdResult = _authService.GetUserId(User);
        if (userIdResult.IsError)
            return userIdResult.Errors.ToActionResult();

        var rank = await _winStreakService.GetRankingAsync(userIdResult.Value, token);
        return Ok(rank);
    }
}
