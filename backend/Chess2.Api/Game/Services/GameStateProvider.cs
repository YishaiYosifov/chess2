using Chess2.Api.ArchivedGames.Services;
using Chess2.Api.GameSnapshot.Models;
using Chess2.Api.LiveGame.Errors;
using Chess2.Api.LiveGame.Grains;
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

public class GameStateProvider(IGameArchiveService gameArchiveService, IGrainFactory grains)
    : IGameStateProvider
{
    private readonly IGameArchiveService _gameArchiveService = gameArchiveService;
    private readonly IGrainFactory _grains = grains;

    public async Task<ErrorOr<GameState>> GetGameStateAsync(
        string gameToken,
        string forUserId,
        CancellationToken token = default
    )
    {
        var liveGameStateResult = await _grains
            .GetGrain<IGameGrain>(gameToken)
            .GetStateAsync(forUserId);

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
