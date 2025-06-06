using Akka.Hosting;
using Chess2.Api.Game.Actors;
using Microsoft.AspNetCore.Mvc;

namespace Chess2.Api.Game.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GameController(IRequiredActor<GameActor> gameActor) : Controller
{
    private readonly IRequiredActor<GameActor> _gameActor = gameActor;

    //[HttpGet("/live/{token}", Name = nameof(GetLiveGame))]
    //public async Task GetLiveGame(string token, CancellationToken token) { }
}
