using Chess2.Api.GameLogic.Models;
using Chess2.Api.GameSnapshot.Models;
using Chess2.Api.LiveGame.SignalR;
using Microsoft.AspNetCore.SignalR;

namespace Chess2.Api.LiveGame.Services;

public interface IGameNotifier
{
    Task JoinGameGroupAsync(string gameToken, string userId, string connectionId);
    Task NotifyDrawStateChangeAsync(string gameToken, DrawState drawState);
    Task NotifyGameEndedAsync(string gameToken, GameResultData result);
    Task NotifyMoveMadeAsync(
        string gameToken,
        MoveSnapshot move,
        int moveNumber,
        ClockSnapshot clocks,
        GameColor sideToMove,
        string sideToMoveUserId,
        IEnumerable<byte> encodedLegalMoves,
        bool hasForcedMoves
    );
}

public class GameNotifier(IHubContext<GameHub, IGameHubClient> hub) : IGameNotifier
{
    private readonly IHubContext<GameHub, IGameHubClient> _hub = hub;

    private static string UserGameGroup(string gameToken, string userId) => $"{gameToken}:{userId}";

    public async Task NotifyMoveMadeAsync(
        string gameToken,
        MoveSnapshot move,
        int moveNumber,
        ClockSnapshot clocks,
        GameColor sideToMove,
        string sideToMoveUserId,
        IEnumerable<byte> legalMoves,
        bool hasForcedMoves
    )
    {
        await _hub.Clients.Group(gameToken).MoveMadeAsync(move, sideToMove, moveNumber, clocks);
        await _hub
            .Clients.Group(UserGameGroup(gameToken, sideToMoveUserId))
            .LegalMovesChangedAsync(legalMoves, hasForcedMoves);
    }

    public Task NotifyDrawStateChangeAsync(string gameToken, DrawState drawState) =>
        _hub.Clients.Group(gameToken).DrawStateChangeAsync(drawState);

    public Task NotifyGameEndedAsync(string gameToken, GameResultData result) =>
        _hub.Clients.Group(gameToken).GameEndedAsync(result);

    public async Task JoinGameGroupAsync(string gameToken, string userId, string connectionId)
    {
        await _hub.Groups.AddToGroupAsync(connectionId, gameToken);
        await _hub.Groups.AddToGroupAsync(connectionId, UserGameGroup(gameToken, userId));
    }
}
