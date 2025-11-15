using AnarchyChess.Api.Donations.Services;
using AnarchyChess.Api.Infrastructure.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace AnarchyChess.Api.Donations.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DonationsController(IKofiWebhookService kofiWebhookService) : Controller
{
    private readonly IKofiWebhookService _kofiWebhookService = kofiWebhookService;

    [HttpPost("webhook")]
    public async Task<ActionResult> KofiWebhook([FromForm] string data)
    {
        var result = await _kofiWebhookService.ReceiveWebhookAsync(data);
        return result.Match(onValue: value => Ok(), onError: errors => errors.ToActionResult());
    }
}
