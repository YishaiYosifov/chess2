using Chess2.Api.Game.Models;
using Chess2.Api.Game.Services;
using Chess2.Api.GameLogic.Models;
using Chess2.Api.Infrastructure;
using Chess2.Api.Infrastructure.SignalR;
using ErrorOr;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Chess2.Api.Game.SignalR;

public interface IGameHubClient : IChess2HubClient
{
    Task MoveMadeAsync(string move, GameColor sideToMove, int moveNumber);

    Task LegalMovesChangedAsync(IEnumerable<string> legalMoves);

    Task GameEndedAsync(
        GameResult result,
        string resultDescription,
        int? newWhiteRating,
        int? newBlackRating
    );
}

[Authorize(AuthPolicies.AuthedSesssion)]
public class GameHub(ILogger<GameHub> logger, IGameService gameService) : Chess2Hub<IGameHubClient>
{
    private const string GameTokenQueryParam = "gameToken";

    private readonly ILogger<GameHub> _logger = logger;
    private readonly IGameService _gameService = gameService;

    public async Task MovePieceAsync(string gameToken, AlgebraicPoint from, AlgebraicPoint to)
    {
        if (!TryGetUserId(out var userId))
        {
            await HandleErrors(Error.Unauthorized());
            return;
        }

        var response = await _gameService.MakeMoveAsync(gameToken, userId, from, to);
        if (response.IsError)
        {
            await HandleErrors(response.Errors);
            return;
        }
    }

    public async Task EndGameAsync(string gameToken)
    {
        if (!TryGetUserId(out var userId))
        {
            await HandleErrors(Error.Unauthorized());
            return;
        }

        var response = await _gameService.EndGameAsync(gameToken, userId);
        if (response.IsError)
        {
            await HandleErrors(response.Errors);
            return;
        }
    }

    public override async Task OnConnectedAsync()
    {
        string? gameToken = Context.GetHttpContext()?.Request.Query[GameTokenQueryParam];
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

        var isGameOngoing = await _gameService.IsGameOngoingAsync(gameToken);
        if (!isGameOngoing)
        {
            _logger.LogWarning(
                "User {UserId} connected to game hub for a game that is not ongoing",
                Context.UserIdentifier
            );
            Context.Abort();
            await base.OnConnectedAsync();
            return;
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, gameToken);
        await base.OnConnectedAsync();
    }
}
