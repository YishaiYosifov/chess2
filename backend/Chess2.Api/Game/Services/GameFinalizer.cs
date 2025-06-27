using Chess2.Api.Game.Entities;
using Chess2.Api.Game.Models;
using Chess2.Api.Matchmaking.Services;
using Chess2.Api.UserRating.Services;
using Chess2.Api.Users.Entities;
using Microsoft.AspNetCore.Identity;

namespace Chess2.Api.Game.Services;

public interface IGameFinalizer
{
    Task<GameArchive?> FinalizeGameAsync(
        string gameToken,
        GameState gameState,
        GameResult gameResult,
        CancellationToken token = default
    );
}

public class GameFinalizer(
    UserManager<AuthedUser> userManager,
    IRatingService ratingService,
    IGameArchiveService gameArchiveService,
    ITimeControlTranslator timeControlTranslator
) : IGameFinalizer
{
    private readonly UserManager<AuthedUser> _userManager = userManager;
    private readonly IRatingService _ratingService = ratingService;
    private readonly IGameArchiveService _gameArchiveService = gameArchiveService;
    private readonly ITimeControlTranslator _timeControlTranslator = timeControlTranslator;

    public async Task<GameArchive?> FinalizeGameAsync(
        string gameToken,
        GameState gameState,
        GameResult gameResult,
        CancellationToken token = default
    )
    {
        if (gameResult is GameResult.Aborted)
            return null;

        var whiteUser = await _userManager.FindByIdAsync(gameState.WhitePlayer.UserId);
        var blackUser = await _userManager.FindByIdAsync(gameState.BlackPlayer.UserId);
        int whiteRatingDelta = 0;
        if (whiteUser is not null && blackUser is not null)
        {
            whiteRatingDelta = await _ratingService.UpdateRatingForResultAsync(
                whiteUser,
                blackUser,
                gameResult,
                _timeControlTranslator.FromSeconds(gameState.TimeControl.BaseSeconds),
                token
            );
        }
        var archive = await _gameArchiveService.CreateArchiveAsync(
            gameToken,
            gameState,
            gameResult,
            whiteRatingDelta,
            token
        );

        return archive;
    }
}
