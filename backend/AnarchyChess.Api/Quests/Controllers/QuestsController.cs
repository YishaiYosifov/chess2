using AnarchyChess.Api.Auth.Services;
using AnarchyChess.Api.ErrorHandling.Extensions;
using AnarchyChess.Api.ErrorHandling.Infrastructure;
using AnarchyChess.Api.Infrastructure.Extensions;
using AnarchyChess.Api.Pagination.Models;
using AnarchyChess.Api.Quests.DTOs;
using AnarchyChess.Api.Quests.Grains;
using AnarchyChess.Api.Quests.Services;
using AnarchyChess.Api.Shared.Models;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AnarchyChess.Api.Quests.Controllers;

[ApiController]
[Route("api/[controller]")]
public class QuestsController(
    IGrainFactory grains,
    IAuthService authService,
    IQuestService questService,
    IValidator<PaginationQuery> paginationValidator
) : Controller
{
    private readonly IGrainFactory _grains = grains;
    private readonly IAuthService _authService = authService;
    private readonly IQuestService _questService = questService;
    private readonly IValidator<PaginationQuery> _paginationValidator = paginationValidator;

    [HttpGet]
    [ProducesResponseType<QuestDto>(StatusCodes.Status200OK)]
    [Authorize]
    public async Task<ActionResult<QuestDto>> GetDailyQuest()
    {
        var userIdResult = _authService.GetUserId(User);
        if (userIdResult.IsError)
            return userIdResult.Errors.ToActionResult();

        var quest = await _grains.GetGrain<IQuestGrain>(userIdResult.Value).GetQuestAsync();
        return Ok(quest);
    }

    [HttpPost("replace")]
    [ProducesResponseType<QuestDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiProblemDetails>(StatusCodes.Status403Forbidden)]
    [Authorize]
    public async Task<ActionResult<QuestDto>> ReplaceDailyQuest()
    {
        var userIdResult = _authService.GetUserId(User);
        if (userIdResult.IsError)
            return userIdResult.Errors.ToActionResult();

        var replaceResult = await _grains
            .GetGrain<IQuestGrain>(userIdResult.Value)
            .ReplaceQuestAsync();
        return replaceResult.Match(Ok, errors => errors.ToActionResult());
    }

    [HttpPost("claim")]
    [ProducesResponseType<int>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiProblemDetails>(StatusCodes.Status404NotFound)]
    [Authorize]
    public async Task<ActionResult<int>> CollectQuestReward()
    {
        var userIdResult = _authService.GetUserId(User);
        if (userIdResult.IsError)
            return userIdResult.Errors.ToActionResult();

        var replaceResult = await _grains
            .GetGrain<IQuestGrain>(userIdResult.Value)
            .CollectRewardAsync();
        return replaceResult.Match(value => Ok(value), errors => errors.ToActionResult());
    }

    [HttpGet("points/{userId}")]
    [ProducesResponseType<int>(StatusCodes.Status200OK)]
    public async Task<ActionResult<int>> GetUserQuestPoints(
        string userId,
        CancellationToken token = default
    )
    {
        var points = await _questService.GetQuestPointsAsync(userId, token);
        return Ok(points);
    }

    [HttpGet("leaderboard")]
    [ProducesResponseType<PagedResult<QuestPointsDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<QuestPointsDto>>> GetQuestLeaderboard(
        [FromQuery] PaginationQuery pagination,
        CancellationToken token = default
    )
    {
        var validationResult = _paginationValidator.Validate(pagination);
        if (!validationResult.IsValid)
            return validationResult.Errors.ToErrorList().ToActionResult();

        var leaderboard = await _questService.GetPaginatedLeaderboardAsync(pagination, token);
        return Ok(leaderboard);
    }

    [HttpGet("leaderboard/me")]
    [ProducesResponseType<int>(StatusCodes.Status200OK)]
    [Authorize]
    public async Task<ActionResult<int>> GetMyQuestRanking(CancellationToken token = default)
    {
        var userIdResult = _authService.GetUserId(User);
        if (userIdResult.IsError)
            return userIdResult.Errors.ToActionResult();

        var ranking = await _questService.GetRankingAsync(userIdResult.Value, token);
        return Ok(ranking);
    }
}
