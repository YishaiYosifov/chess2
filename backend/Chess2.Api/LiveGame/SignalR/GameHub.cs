using Chess2.Api.GameLogic.Models;
using Chess2.Api.GameSnapshot.Models;
using Chess2.Api.Infrastructure;
using Chess2.Api.Infrastructure.SignalR;
using Chess2.Api.LiveGame.Grains;
using Chess2.Api.LiveGame.Models;
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

    Task DrawStateChangeAsync(DrawState drawState);
    Task GameEndedAsync(GameResultData result);

    Task ChatMessageAsync(string senderUserId, string senderUsername, string message);
    Task ChatConnectedAsync();
    Task ChatMessageDeliveredAsync(double cooldownLeftMs);
}

[Authorize(AuthPolicies.ActiveSession)]
public class GameHub(ILogger<GameHub> logger, IGrainFactory grains, IGameNotifier gameNotifier)
    : Chess2Hub<IGameHubClient>
{
    private const string GameTokenQueryParam = "gameToken";

    private readonly ILogger<GameHub> _logger = logger;
    private readonly IGrainFactory _grains = grains;
    private readonly IGameNotifier _gameNotifier = gameNotifier;

    public async Task MovePieceAsync(string gameToken, MoveKey key)
    {
        if (!TryGetUserId(out var userId))
        {
            await HandleErrors(Error.Unauthorized());
            return;
        }

        var response = await _grains.GetGrain<IGameGrain>(gameToken).MovePieceAsync(userId, key);
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

        var response = await _grains.GetGrain<IGameGrain>(gameToken).RequestGameEndAsync(userId);
        if (response.IsError)
        {
            await HandleErrors(response.Errors);
            return;
        }
    }

    public async Task RequestDrawAsync(string gameToken)
    {
        if (!TryGetUserId(out var userId))
        {
            await HandleErrors(Error.Unauthorized());
            return;
        }

        var response = await _grains.GetGrain<IGameGrain>(gameToken).RequestDrawAsync(userId);
        if (response.IsError)
        {
            await HandleErrors(response.Errors);
            return;
        }
    }

    public async Task DeclineDrawAsync(string gameToken)
    {
        if (!TryGetUserId(out var userId))
        {
            await HandleErrors(Error.Unauthorized());
            return;
        }

        var response = await _grains.GetGrain<IGameGrain>(gameToken).DeclineDrawAsync(userId);
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

        var chatGrain = _grains.GetGrain<IGameChatGrain>(gameToken);
        var sendChatResult = await chatGrain.SendMessageAsync(
            connectionId: Context.ConnectionId,
            userId: userId,
            message: message
        );
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

        if (!TryGetUserId(out var userId))
        {
            await HandleErrors(Error.Unauthorized());
            await base.OnConnectedAsync();
            return;
        }

        await _gameNotifier.JoinGameGroupAsync(gameToken, userId, Context.ConnectionId);

        var chatGrain = _grains.GetGrain<IGameChatGrain>(gameToken);
        await chatGrain.JoinChatAsync(connectionId: Context.ConnectionId, userId: userId);
        await Clients.Caller.ChatConnectedAsync();

        await base.OnConnectedAsync();
    }
}
