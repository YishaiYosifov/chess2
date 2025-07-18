using Chess2.Api.ArchivedGames.Services;
using Chess2.Api.GameSnapshot.Models;
using Chess2.Api.LiveGame.Errors;
using Chess2.Api.LiveGame.Services;
using ErrorOr;

namespace Chess2.Api.Game.Services;

public interface IGameStateProvider
{
    Task<ErrorOr<GameState>> GetGameStateAsync(
        string gameToken,
        string forUserId,
        CancellationToken token = default
    );
}

public class GameStateProvider(
    ILiveGameService liveGameService,
    IGameArchiveService gameArchiveService
) : IGameStateProvider
{
    private readonly ILiveGameService _liveGameService = liveGameService;
    private readonly IGameArchiveService _gameArchiveService = gameArchiveService;

    public async Task<ErrorOr<GameState>> GetGameStateAsync(
        string gameToken,
        string forUserId,
        CancellationToken token = default
    )
    {
        var liveGameStateResult = await _liveGameService.GetGameStateAsync(
            gameToken,
            forUserId,
            token
        );
        if (!liveGameStateResult.IsError)
            return liveGameStateResult.Value;
        if (!liveGameStateResult.Errors.Contains(GameErrors.GameNotFound))
            return liveGameStateResult.Errors;

        var archivedStateResult = await _gameArchiveService.GetGameStateByTokenAsync(
            gameToken,
            token
        );
        return archivedStateResult;
    }
}
