using Chess2.Api.GameLogic.Models;
using Chess2.Api.GameSnapshot.Models;
using Chess2.Api.Infrastructure;
using Chess2.Api.Infrastructure.SignalR;
using Chess2.Api.LiveGame.Services;
using ErrorOr;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Chess2.Api.LiveGame.SignalR;

public interface IGameHubClient : IChess2HubClient
{
    Task MoveMadeAsync(
        MoveSnapshot move,
        GameColor sideToMove,
        int moveNumber,
        ClockSnapshot clock
    );

    Task LegalMovesChangedAsync(IEnumerable<byte> encodedLegalMoves, bool hasForcedMoves);

    Task GameEndedAsync(GameResultData result);

    Task ChatMessageAsync(string sender, string message);
    Task ChatConnectedAsync();
    Task ChatMessageDeliveredAsync(double cooldownLeftMs);
}

[Authorize(AuthPolicies.AuthedSesssion)]
public class GameHub(
    ILogger<GameHub> logger,
    ILiveGameService gameService,
    IGameChatService gameChatService
) : Chess2Hub<IGameHubClient>
{
    private const string GameTokenQueryParam = "gameToken";

    private readonly ILogger<GameHub> _logger = logger;
    private readonly ILiveGameService _gameService = gameService;
    private readonly IGameChatService _gameChatService = gameChatService;

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

    public async Task SendChatAsync(string gameToken, string message)
    {
        if (!TryGetUserId(out var userId))
        {
            await HandleErrors(Error.Unauthorized());
            return;
        }

        var sendChatResult = await _gameChatService.SendMessage(gameToken, userId, message);
        if (sendChatResult.IsError)
        {
            await HandleErrors(sendChatResult.Errors);
            return;
        }
    }

    public override async Task OnConnectedAsync()
    {
        string? gameToken = Context.GetHttpContext()?.Request.Query[GameTokenQueryParam];
        if (gameToken is null)
        {
            await HandleErrors(
                Error.Validation($"Missing required query parameter: {GameTokenQueryParam}")
            );
            await base.OnConnectedAsync();
            return;
        }

        await _gameChatService.JoinChat(gameToken, Context.ConnectionId, Context.User);
        await Clients.Caller.ChatConnectedAsync();

        await Groups.AddToGroupAsync(Context.ConnectionId, gameToken);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        string? gameToken = Context.GetHttpContext()?.Request.Query[GameTokenQueryParam];
        if (gameToken is null)
        {
            await base.OnDisconnectedAsync(exception);
            return;
        }

        if (!TryGetUserId(out var userId))
        {
            await HandleErrors(Error.Unauthorized());
            return;
        }

        await _gameChatService.LeaveChat(gameToken, userId);
    }
}
