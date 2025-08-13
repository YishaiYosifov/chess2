using Chess2.Api.ArchivedGames.Services;
using Chess2.Api.GameSnapshot.Models;
using Chess2.Api.GameSnapshot.Services;
using Chess2.Api.Lobby.Grains;
using Chess2.Api.Shared.Services;
using Chess2.Api.UserRating.Models;
using Chess2.Api.UserRating.Services;
using Chess2.Api.Users.Entities;
using Microsoft.AspNetCore.Identity;

namespace Chess2.Api.LiveGame.Services;

public interface IGameFinalizer
{
    Task<GameResultData> FinalizeGameAsync(
        string gameToken,
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
    IGrainFactory grainFactory,
    IUnitOfWork unitOfWork
) : IGameFinalizer
{
    private readonly UserManager<AuthedUser> _userManager = userManager;
    private readonly IRatingService _ratingService = ratingService;
    private readonly IGameArchiveService _gameArchiveService = gameArchiveService;
    private readonly ITimeControlTranslator _timeControlTranslator = timeControlTranslator;
    private readonly IGrainFactory _grainFactory = grainFactory;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<GameResultData> FinalizeGameAsync(
        string gameToken,
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

        await _grainFactory
            .GetGrain<IPlayerSessionGrain>(state.WhitePlayer.UserId)
            .GameEndedAsync(gameToken);
        await _grainFactory
            .GetGrain<IPlayerSessionGrain>(state.BlackPlayer.UserId)
            .GameEndedAsync(gameToken);

        await _unitOfWork.CompleteAsync(token);

        return new(
            Result: endStatus.Result,
            ResultDescription: endStatus.ResultDescription,
            WhiteRatingChange: ratingChange?.WhiteChange,
            BlackRatingChange: ratingChange?.BlackChange
        );
    }

    private async Task<RatingChange?> UpdateRatingAsync(
        GameState gameState,
        GameResult gameResult,
        CancellationToken token = default
    )
    {
        if (!gameState.IsRated || gameResult is GameResult.Aborted)
            return null;

        var whiteUser = await _userManager.FindByIdAsync(gameState.WhitePlayer.UserId);
        var blackUser = await _userManager.FindByIdAsync(gameState.BlackPlayer.UserId);
        if (whiteUser is null || blackUser is null)
            return null;

        var ratingChange = await _ratingService.UpdateRatingForResultAsync(
            whiteUser,
            blackUser,
            gameResult,
            _timeControlTranslator.FromSeconds(gameState.TimeControl.BaseSeconds),
            token
        );
        return ratingChange;
    }
}
