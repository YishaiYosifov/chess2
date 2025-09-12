using Chess2.Api.Auth.Services;
using Chess2.Api.Infrastructure.Extensions;
using Chess2.Api.Quests.DTOs;
using Chess2.Api.Quests.Grains;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Chess2.Api.Quests.Controllers;

[ApiController]
[Route("api/[controller]")]
public class QuestController(IGrainFactory grains, IAuthService authService) : Controller
{
    private readonly IGrainFactory _grains = grains;
    private readonly IAuthService _authService = authService;

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
}
