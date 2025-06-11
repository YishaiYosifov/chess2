using Chess2.Api.Game.Services;
using Chess2.Api.GameLogic.Models;
using Chess2.Api.Infrastructure;
using Chess2.Api.Infrastructure.SignalR;
using ErrorOr;
using Microsoft.AspNetCore.Authorization;

namespace Chess2.Api.Game.SignalR;

public interface IGameHub : IChess2HubClient { }

[Authorize(AuthPolicies.AuthedSesssion)]
public class GameHub(ILogger<GameHub> logger, IGameService gameService) : Chess2Hub<IGameHub>
{
    private readonly ILogger<GameHub> _logger = logger;
    private readonly IGameService _gameService = gameService;

    public async Task MovePieceAsync(string gameToken, Point from, Point to)
    {
        if (!TryGetUserId(out var userId))
        {
            await HandleErrors(Error.Unauthorized());
            return;
        }

        await _gameService.PerformMoveAsync(gameToken, userId, from, to);
    }
}
