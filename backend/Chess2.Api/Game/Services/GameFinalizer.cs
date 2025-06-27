using Chess2.Api.Game.Models;
using Chess2.Api.Matchmaking.Services;
using Chess2.Api.UserRating.Services;
using Chess2.Api.Users.Entities;
using Microsoft.AspNetCore.Identity;

namespace Chess2.Api.Game.Services;

public interface IGameFinalizer
{
    Task FinalizeGameAsync(
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

    public async Task FinalizeGameAsync(
        string gameToken,
        GameState gameState,
        GameResult gameResult,
        CancellationToken token = default
    )
    {
        if (gameResult is GameResult.Aborted)
            return;

        await _gameArchiveService.CreateArchiveAsync(gameToken, gameState, gameResult, token);

        var whiteUser = await _userManager.FindByIdAsync(gameState.WhitePlayer.UserId);
        var blackUser = await _userManager.FindByIdAsync(gameState.BlackPlayer.UserId);
        if (whiteUser is not null && blackUser is not null)
        {
            await _ratingService.UpdateRatingForResultAsync(
                whiteUser,
                blackUser,
                gameResult,
                _timeControlTranslator.FromSeconds(gameState.TimeControl.BaseSeconds),
                token
            );
        }
    }
}
