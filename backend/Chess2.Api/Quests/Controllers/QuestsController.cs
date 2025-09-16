using Chess2.Api.Auth.Services;
using Chess2.Api.Infrastructure.Errors;
using Chess2.Api.Infrastructure.Extensions;
using Chess2.Api.Pagination.Models;
using Chess2.Api.Quests.DTOs;
using Chess2.Api.Quests.Grains;
using Chess2.Api.Quests.Services;
using Chess2.Api.Shared.Models;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Chess2.Api.Quests.Controllers;

[ApiController]
[Route("api/[controller]")]
public class QuestsController(
    IGrainFactory grains,
    IAuthService authService,
    IQuestService questLeaderboard,
    IValidator<PaginationQuery> paginationValidator
) : Controller
{
    private readonly IGrainFactory _grains = grains;
    private readonly IAuthService _authService = authService;
    private readonly IQuestService _questLeaderboard = questLeaderboard;
    private readonly IValidator<PaginationQuery> _paginationValidator = paginationValidator;

    [HttpGet(Name = nameof(GetDailyQuest))]
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

    [HttpPost("replace", Name = nameof(ReplaceDailyQuest))]
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

    [HttpPost("claim", Name = nameof(CollectQuestReward))]
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

    [HttpGet("leaderboard", Name = nameof(GetQuestLeaderboard))]
    [ProducesResponseType<PagedResult<QuestPointsDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<QuestPointsDto>>> GetQuestLeaderboard(
        [FromQuery] PaginationQuery pagination,
        CancellationToken token = default
    )
    {
        var validationResult = _paginationValidator.Validate(pagination);
        if (!validationResult.IsValid)
            return validationResult.Errors.ToErrorList().ToActionResult();

        var leaderboard = await _questLeaderboard.GetPaginatedLeaderboardAsync(pagination, token);
        return Ok(leaderboard);
    }

    [HttpGet("leaderboard/me", Name = nameof(GetMyQuestRanking))]
    [ProducesResponseType<int>(StatusCodes.Status200OK)]
    [Authorize]
    public async Task<ActionResult<int>> GetMyQuestRanking(CancellationToken token = default)
    {
        var userIdResult = _authService.GetUserId(User);
        if (userIdResult.IsError)
            return userIdResult.Errors.ToActionResult();

        var ranking = await _questLeaderboard.GetRankingAsync(userIdResult.Value, token);
        return Ok(ranking);
    }
}
