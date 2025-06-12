using Chess2.Api.Game.Services;
using Chess2.Api.GameLogic.Models;
using Chess2.Api.Infrastructure;
using Chess2.Api.Infrastructure.SignalR;
using ErrorOr;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Chess2.Api.Game.SignalR;

public interface IGameHub : IChess2HubClient { }

[Authorize(AuthPolicies.AuthedSesssion)]
public class GameHub(ILogger<GameHub> logger, IGameService gameService) : Chess2Hub<IGameHub>
{
    private const string GameTokenQueryParm = "gameToken";

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

    public override async Task OnConnectedAsync()
    {
        string? gameToken = Context.GetHttpContext()?.Request.Query[GameTokenQueryParm];
        if (gameToken is null)
        {
            _logger.LogWarning(
                "User {UserId} connected to game hub without a game token",
                Context.UserIdentifier
            );
            Context.Abort();
            await base.OnConnectedAsync();
            return;
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, gameToken);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        string? gameToken = Context.GetHttpContext()?.Request.Query[GameTokenQueryParm];
        if (gameToken is not null)
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, gameToken);

        await base.OnDisconnectedAsync(exception);
    }
}
