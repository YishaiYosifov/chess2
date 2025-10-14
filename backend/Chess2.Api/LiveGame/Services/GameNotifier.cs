using Chess2.Api.GameLogic.Models;
using Chess2.Api.GameSnapshot.Models;
using Chess2.Api.LiveGame.Models;
using Chess2.Api.LiveGame.SignalR;
using Chess2.Api.Profile.Models;
using Chess2.Api.Shared.Models;
using Microsoft.AspNetCore.SignalR;

namespace Chess2.Api.LiveGame.Services;

public interface IGameNotifier
{
    Task SyncCurrentMoveAsync(GameState gameState, ConnectionId connectionId);
    Task JoinGameGroupAsync(GameToken gameToken, UserId userId, ConnectionId connectionId);
    Task NotifyDrawStateChangeAsync(GameToken gameToken, DrawState drawState);
    Task NotifyGameEndedAsync(GameToken gameToken, GameResultData result);
    Task NotifyMoveMadeAsync(
        GameToken gameToken,
        MoveSnapshot move,
        int moveNumber,
        ClockSnapshot clocks,
        GameColor sideToMove,
        UserId sideToMoveUserId,
        IEnumerable<byte> encodedLegalMoves,
        bool hasForcedMoves
    );
}

public class GameNotifier(IHubContext<GameHub, IGameHubClient> hub) : IGameNotifier
{
    private readonly IHubContext<GameHub, IGameHubClient> _hub = hub;

    private static string UserGameGroup(GameToken gameToken, UserId userId) =>
        $"{gameToken}:{userId}";

    public Task SyncCurrentMoveAsync(GameState gameState, ConnectionId connectionId) =>
        _hub.Clients.Client(connectionId).SyncGameStateAsync(gameState);

    public async Task NotifyMoveMadeAsync(
        GameToken gameToken,
        MoveSnapshot move,
        int moveNumber,
        ClockSnapshot clocks,
        GameColor sideToMove,
        UserId sideToMoveUserId,
        IEnumerable<byte> legalMoves,
        bool hasForcedMoves
    )
    {
        await _hub.Clients.Group(gameToken).MoveMadeAsync(move, sideToMove, moveNumber, clocks);
        await _hub
            .Clients.Group(UserGameGroup(gameToken, sideToMoveUserId))
            .LegalMovesChangedAsync(legalMoves, hasForcedMoves);
    }

    public Task NotifyDrawStateChangeAsync(GameToken gameToken, DrawState drawState) =>
        _hub.Clients.Group(gameToken).DrawStateChangeAsync(drawState);

    public Task NotifyGameEndedAsync(GameToken gameToken, GameResultData result) =>
        _hub.Clients.Group(gameToken).GameEndedAsync(result);

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
