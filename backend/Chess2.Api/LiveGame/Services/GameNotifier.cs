using Chess2.Api.GameLogic.Models;
using Chess2.Api.GameSnapshot.Models;
using Chess2.Api.LiveGame.SignalR;
using Microsoft.AspNetCore.SignalR;

namespace Chess2.Api.LiveGame.Services;

public interface IGameNotifier
{
    Task NotifyGameEndedAsync(string gameToken, GameResultData result);
    Task NotifyMoveMadeAsync(
        string gameToken,
        MoveSnapshot move,
        int moveNumber,
        ClockSnapshot clocks,
        GameColor sideToMove,
        string sideToMoveUserId,
        IEnumerable<byte> encodedLegalMoves
    );
}

public class GameNotifier(IHubContext<GameHub, IGameHubClient> hub) : IGameNotifier
{
    private readonly IHubContext<GameHub, IGameHubClient> _hub = hub;

    public async Task NotifyMoveMadeAsync(
        string gameToken,
        MoveSnapshot move,
        int moveNumber,
        ClockSnapshot clocks,
        GameColor sideToMove,
        string sideToMoveUserId,
        IEnumerable<byte> legalMoves
    )
    {
        await _hub.Clients.Group(gameToken).MoveMadeAsync(move, sideToMove, moveNumber, clocks);
        await _hub.Clients.User(sideToMoveUserId).LegalMovesChangedAsync(legalMoves);
    }

    public Task NotifyGameEndedAsync(string gameToken, GameResultData result) =>
        _hub.Clients.Group(gameToken).GameEndedAsync(result);
}
