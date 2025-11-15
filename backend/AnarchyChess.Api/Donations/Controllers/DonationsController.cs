using AnarchyChess.Api.Donations.Services;
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
        await _kofiWebhookService.ReceiveWebhookAsync(data);
        return Ok();
    }
}
