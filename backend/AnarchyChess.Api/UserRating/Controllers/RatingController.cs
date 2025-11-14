using AnarchyChess.Api.Infrastructure.Errors;
using AnarchyChess.Api.Infrastructure.Extensions;
using AnarchyChess.Api.Profile.Entities;
using AnarchyChess.Api.UserRating.Models;
using AnarchyChess.Api.UserRating.Services;
using ErrorOr;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AnarchyChess.Api.UserRating.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RatingController(IRatingService ratingService, UserManager<AuthedUser> userManager)
    : Controller
{
    private readonly IRatingService _ratingService = ratingService;
    private readonly UserManager<AuthedUser> _userManager = userManager;

    [HttpGet("{userId}/archive")]
    [ProducesResponseType<IEnumerable<RatingOverview>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<RatingOverview>>> GetRatingArchives(
        string userId,
        [FromQuery] DateTime? since,
        CancellationToken token
    )
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
            return Error.NotFound().ToActionResult();

        var ratings = await _ratingService.GetRatingOverviewsAsync(user, since, token);
        return Ok(ratings);
    }

    [HttpGet("{userId}")]
    [ProducesResponseType<IEnumerable<CurrentRatingStatus>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<CurrentRatingStatus>>> GetCurrentRatings(
        string userId,
        CancellationToken token
    )
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
            return Error.NotFound().ToActionResult();

        var ratings = await _ratingService.GetCurrentRatingsAsync(user, token);
        return Ok(ratings);
    }
}
