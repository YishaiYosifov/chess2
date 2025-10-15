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
    Task SyncRevisionAsync(int currentRevision);
    Task MoveMadeAsync(
        MoveSnapshot move,
        GameColor sideToMove,
        int moveNumber,
        ClockSnapshot clock
    );
    Task LegalMovesChangedAsync(IEnumerable<byte> encodedLegalMoves, bool hasForcedMoves);

    Task DrawStateChangeAsync(DrawState drawState);
    Task GameEndedAsync(GameResultData result);

    Task ChatMessageAsync(string senderUserName, string message);
    Task ChatConnectedAsync();
    Task ChatMessageDeliveredAsync(double cooldownLeftMs);

    Task RematchRequestedAsync();
    Task RematchCancelledAsync();
    Task RematchAccepted(GameToken createdGameToken);
}

[Authorize(AuthPolicies.ActiveSession)]
public class GameHub(ILogger<GameHub> logger, IGrainFactory grains, IGameNotifier gameNotifier)
    : Chess2Hub<IGameHubClient>
{
    private const string GameTokenQueryParam = "gameToken";

    private readonly ILogger<GameHub> _logger = logger;
    private readonly IGrainFactory _grains = grains;
    private readonly IGameNotifier _gameNotifier = gameNotifier;

    public async Task MovePieceAsync(GameToken gameToken, string key)
    {
        if (!TryGetUserId(out var userId))
        {
            await HandleErrors(Error.Unauthorized());
            return;
        }

        var result = await _grains.GetGrain<IGameGrain>(gameToken).MovePieceAsync(userId, key);
        if (result.IsError)
        {
            await HandleErrors(result.Errors);
            return;
        }
    }

    public async Task EndGameAsync(GameToken gameToken)
    {
        if (!TryGetUserId(out var userId))
        {
            await HandleErrors(Error.Unauthorized());
            return;
        }

        var result = await _grains.GetGrain<IGameGrain>(gameToken).RequestGameEndAsync(userId);
        if (result.IsError)
        {
            await HandleErrors(result.Errors);
            return;
        }
    }

    public async Task RequestDrawAsync(GameToken gameToken)
    {
        if (!TryGetUserId(out var userId))
        {
            await HandleErrors(Error.Unauthorized());
            return;
        }

        var result = await _grains.GetGrain<IGameGrain>(gameToken).RequestDrawAsync(userId);
        if (result.IsError)
        {
            await HandleErrors(result.Errors);
            return;
        }
    }

    public async Task DeclineDrawAsync(GameToken gameToken)
    {
        if (!TryGetUserId(out var userId))
        {
            await HandleErrors(Error.Unauthorized());
            return;
        }

        var result = await _grains.GetGrain<IGameGrain>(gameToken).DeclineDrawAsync(userId);
        if (result.IsError)
        {
            await HandleErrors(result.Errors);
            return;
        }
    }

    public async Task SendChatAsync(GameToken gameToken, string message)
    {
        if (!TryGetUserId(out var userId))
        {
            await HandleErrors(Error.Unauthorized());
            return;
        }

        var chatGrain = _grains.GetGrain<IGameChatGrain>(gameToken);
        var result = await chatGrain.SendMessageAsync(
            connectionId: Context.ConnectionId,
            userId: userId,
            message: message
        );
        if (result.IsError)
        {
            await HandleErrors(result.Errors);
            return;
        }
    }

    public async Task RequestRematchAsync(GameToken gameToken)
    {
        if (!TryGetUserId(out var userId))
        {
            await HandleErrors(Error.Unauthorized());
            return;
        }

        var rematchGrain = _grains.GetGrain<IRematchGrain>(gameToken);
        var result = await rematchGrain.RequestAsync(
            requestedBy: userId,
            connectionId: Context.ConnectionId
        );
        if (result.IsError)
        {
            await HandleErrors(result.Errors);
            return;
        }
    }

    public async Task CancelRematchAsync(GameToken gameToken)
    {
        if (!TryGetUserId(out var userId))
        {
            await HandleErrors(Error.Unauthorized());
            return;
        }

        var rematchGrain = _grains.GetGrain<IRematchGrain>(gameToken);
        var result = await rematchGrain.CancelAsync(cancelledBy: userId);
        if (result.IsError)
        {
            await HandleErrors(result.Errors);
            return;
        }
    }

    public override async Task OnConnectedAsync()
    {
        try
        {
            string? gameToken = Context.GetHttpContext()?.Request.Query[GameTokenQueryParam];
            if (string.IsNullOrWhiteSpace(gameToken))
            {
                await HandleErrors(
                    Error.Validation($"Missing required query parameter: {GameTokenQueryParam}")
                );
                return;
            }

            if (!TryGetUserId(out var userId))
            {
                await HandleErrors(Error.Unauthorized());
                return;
            }

            await _gameNotifier.JoinGameGroupAsync(gameToken, userId, Context.ConnectionId);

            var gameGrain = _grains.GetGrain<IGameGrain>(gameToken);
            await gameGrain.SyncRevisionAsync(Context.ConnectionId);

            var chatGrain = _grains.GetGrain<IGameChatGrain>(gameToken);
            await chatGrain.JoinChatAsync(connectionId: Context.ConnectionId, userId: userId);
            await Clients.Caller.ChatConnectedAsync();
        }
        finally
        {
            await base.OnConnectedAsync();
        }
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        try
        {
            string? gameToken = Context.GetHttpContext()?.Request.Query[GameTokenQueryParam];
            if (gameToken is null)
                return;

            if (!TryGetUserId(out var userId))
                return;

            var rematchGrain = _grains.GetGrain<IRematchGrain>(gameToken);
            await rematchGrain.RemoveConnectionAsync(ofUserId: userId, Context.ConnectionId);
        }
        finally
        {
            await base.OnDisconnectedAsync(exception);
        }
    }
}
