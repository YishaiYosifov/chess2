using AnarchyChess.Api.Donations.Repositories;
using AnarchyChess.Api.Donations.Services;
using AnarchyChess.Api.Infrastructure.Errors;
using AnarchyChess.Api.Infrastructure.Extensions;
using AnarchyChess.Api.Pagination.Models;
using AnarchyChess.Api.Shared.Models;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace AnarchyChess.Api.Donations.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DonationsController(
    IKofiWebhookService kofiWebhookService,
    IDonationWallService donationWallService,
    IValidator<PaginationQuery> paginationValidator
) : Controller
{
    private readonly IKofiWebhookService _kofiWebhookService = kofiWebhookService;
    private readonly IDonationWallService _donationWallService = donationWallService;
    private readonly IValidator<PaginationQuery> _paginationValidator = paginationValidator;

    [HttpPost("webhook")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ApiProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> KofiWebhook([FromForm] string data, CancellationToken token)
    {
        var result = await _kofiWebhookService.ReceiveWebhookAsync(data, token);
        return result.Match(onValue: value => Ok(), onError: errors => errors.ToActionResult());
    }

    [HttpGet("wall")]
    [ProducesResponseType<PagedResult<string>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<string>>> GetDonationWall(
        [FromQuery] PaginationQuery paginationQuery,
        CancellationToken token
    )
    {
        var validationResult = _paginationValidator.Validate(paginationQuery);
        if (!validationResult.IsValid)
            return validationResult.Errors.ToErrorList().ToActionResult();

        var wall = await _donationWallService.GetLeaderboardAsync(paginationQuery, token);
        return Ok(wall);
    }
}
