using AnarchyChess.Api.ArchivedGames.Services;
using AnarchyChess.Api.Game.Models;
using AnarchyChess.Api.GameSnapshot.Models;
using AnarchyChess.Api.GameSnapshot.Services;
using AnarchyChess.Api.Matchmaking.Models;
using AnarchyChess.Api.Profile.Entities;
using AnarchyChess.Api.Shared.Services;
using AnarchyChess.Api.UserRating.Models;
using AnarchyChess.Api.UserRating.Services;
using Microsoft.AspNetCore.Identity;

namespace AnarchyChess.Api.Game.Services;

public interface IGameFinalizer
{
    Task<GameResultData> FinalizeGameAsync(
        GameToken gameToken,
        GameState state,
        GameEndStatus endStatus,
        CancellationToken token = default
    );
}

public class GameFinalizer(
    UserManager<AuthedUser> userManager,
    IRatingService ratingService,
    IGameArchiveService gameArchiveService,
    ITimeControlTranslator timeControlTranslator,
    IUnitOfWork unitOfWork
) : IGameFinalizer
{
    private readonly UserManager<AuthedUser> _userManager = userManager;
    private readonly IRatingService _ratingService = ratingService;
    private readonly IGameArchiveService _gameArchiveService = gameArchiveService;
    private readonly ITimeControlTranslator _timeControlTranslator = timeControlTranslator;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<GameResultData> FinalizeGameAsync(
        GameToken gameToken,
        GameState state,
        GameEndStatus endStatus,
        CancellationToken token = default
    )
    {
        var ratingChange = await UpdateRatingAsync(state, endStatus.Result, token);
        await _gameArchiveService.CreateArchiveAsync(
            gameToken,
            state,
            endStatus,
            ratingChange,
            token
        );
        await _unitOfWork.CompleteAsync(token);

        GameResultData result = new(
            Result: endStatus.Result,
            ResultDescription: endStatus.ResultDescription,
            WhiteRatingChange: ratingChange?.WhiteChange,
            BlackRatingChange: ratingChange?.BlackChange
        );
        return result;
    }

    private async Task<RatingChange?> UpdateRatingAsync(
        GameState gameState,
        GameResult gameResult,
        CancellationToken token = default
    )
    {
        if (gameState.Pool.PoolType is not PoolType.Rated || gameResult is GameResult.Aborted)
            return null;

        var whiteUser = await _userManager.FindByIdAsync(gameState.WhitePlayer.UserId);
        var blackUser = await _userManager.FindByIdAsync(gameState.BlackPlayer.UserId);
        if (whiteUser is null || blackUser is null)
            return null;

        var ratingChange = await _ratingService.UpdateRatingForResultAsync(
            whiteUser,
            blackUser,
            gameResult,
            _timeControlTranslator.FromSeconds(gameState.Pool.TimeControl.BaseSeconds),
            token
        );
        return ratingChange;
    }
}
