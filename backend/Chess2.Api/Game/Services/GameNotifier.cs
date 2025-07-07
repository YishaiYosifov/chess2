using Chess2.Api.Game.Models;
using Chess2.Api.Game.SignalR;
using Chess2.Api.GameLogic.Models;
using Microsoft.AspNetCore.SignalR;

namespace Chess2.Api.Game.Services;

public interface IGameNotifier
{
    Task NotifyGameEndedAsync(
        string gameToken,
        GameResult result,
        string resultDescription,
        int? newWhiteRating,
        int? newBlackRating
    );
    Task NotifyMoveMadeAsync(
        string gameToken,
        MoveSnapshot move,
        GameColor sideToMove,
        int moveNumber,
        ClockDto clocks,
        string sideToMoveUserId,
        IEnumerable<string> legalMoves
    );
}

public class GameNotifier(IHubContext<GameHub, IGameHubClient> hub) : IGameNotifier
{
    private readonly IHubContext<GameHub, IGameHubClient> _hub = hub;

    public async Task NotifyMoveMadeAsync(
        string gameToken,
        MoveSnapshot move,
        GameColor sideToMove,
        int moveNumber,
        ClockDto clocks,
        string sideToMoveUserId,
        IEnumerable<string> legalMoves
    )
    {
        await _hub.Clients.Group(gameToken).MoveMadeAsync(move, sideToMove, moveNumber, clocks);
        await _hub.Clients.User(sideToMoveUserId).LegalMovesChangedAsync(legalMoves);
    }

    public Task NotifyGameEndedAsync(
        string gameToken,
        GameResult result,
        string resultDescription,
        int? newWhiteRating,
        int? newBlackRating
    ) =>
        _hub
            .Clients.Group(gameToken)
            .GameEndedAsync(result, resultDescription, newWhiteRating, newBlackRating);
}
