﻿using Chess2.Api.GameLogic.Models;
using Chess2.Api.GameSnapshot.Models;
using Chess2.Api.LiveGame.Models;
using Chess2.Api.LiveGame.SignalR;
using Chess2.Api.Profile.Models;
using Chess2.Api.Shared.Models;
using Microsoft.AspNetCore.SignalR;

namespace Chess2.Api.LiveGame.Services;

public interface IGameNotifier
{
    Task SyncRevisionAsync(ConnectionId connectionId, GameNotifierState state);
    Task JoinGameGroupAsync(GameToken gameToken, UserId userId, ConnectionId connectionId);
    Task NotifyDrawStateChangeAsync(
        GameToken gameToken,
        DrawState drawState,
        GameNotifierState state
    );
    Task NotifyGameEndedAsync(GameToken gameToken, GameResultData result, GameNotifierState state);
    Task NotifyMoveMadeAsync(MoveNotification notification, GameNotifierState state);
}

public record MoveNotification(
    GameToken GameToken,
    MoveSnapshot Move,
    int MoveNumber,
    ClockSnapshot Clocks,
    GameColor SideToMove,
    UserId SideToMoveUserId,
    IReadOnlyCollection<byte> LegalMoves,
    bool HasForcedMoves
);

[GenerateSerializer]
[Alias("Chess2.Api.LiveGame.Services.GameNotifierState")]
public class GameNotifierState
{
    [Id(0)]
    public int Revision { get; set; }
}

public class GameNotifier(IHubContext<GameHub, IGameHubClient> hub) : IGameNotifier
{
    private readonly IHubContext<GameHub, IGameHubClient> _hub = hub;

    private static string UserGameGroup(GameToken gameToken, UserId userId) =>
        $"{gameToken}:{userId}";

    public Task SyncRevisionAsync(ConnectionId connectionId, GameNotifierState state) =>
        _hub.Clients.Client(connectionId).SyncRevisionAsync(state.Revision);

    public async Task NotifyMoveMadeAsync(MoveNotification notification, GameNotifierState state)
    {
        state.Revision++;
        await _hub
            .Clients.Group(notification.GameToken)
            .MoveMadeAsync(
                notification.Move,
                notification.SideToMove,
                notification.MoveNumber,
                notification.Clocks
            );
        await _hub
            .Clients.Group(UserGameGroup(notification.GameToken, notification.SideToMoveUserId))
            .LegalMovesChangedAsync(notification.LegalMoves, notification.HasForcedMoves);
    }

    public Task NotifyDrawStateChangeAsync(
        GameToken gameToken,
        DrawState drawState,
        GameNotifierState state
    )
    {
        state.Revision++;
        return _hub.Clients.Group(gameToken).DrawStateChangeAsync(drawState);
    }

    public Task NotifyGameEndedAsync(
        GameToken gameToken,
        GameResultData result,
        GameNotifierState state
    )
    {
        state.Revision++;
        return _hub.Clients.Group(gameToken).GameEndedAsync(result);
    }

    public async Task JoinGameGroupAsync(
        GameToken gameToken,
        UserId userId,
        ConnectionId connectionId
    )
    {
        await _hub.Groups.AddToGroupAsync(connectionId, gameToken);
        await _hub.Groups.AddToGroupAsync(connectionId, UserGameGroup(gameToken, userId));
    }
}
