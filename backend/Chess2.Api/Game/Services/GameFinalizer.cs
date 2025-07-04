using Akka.Actor;
using Akka.Hosting;
using Chess2.Api.Game.Entities;
using Chess2.Api.Game.Models;
using Chess2.Api.Matchmaking.Services;
using Chess2.Api.Player.Actors;
using Chess2.Api.Player.Models;
using Chess2.Api.Shared.Services;
using Chess2.Api.UserRating.Models;
using Chess2.Api.UserRating.Services;
using Chess2.Api.Users.Entities;
using Microsoft.AspNetCore.Identity;

namespace Chess2.Api.Game.Services;

public interface IGameFinalizer
{
    Task<GameArchive> FinalizeGameAsync(
        string gameToken,
        GameState state,
        GameResult result,
        string resultDescription,
        CancellationToken token = default
    );
}

public class GameFinalizer(
    UserManager<AuthedUser> userManager,
    IRatingService ratingService,
    IGameArchiveService gameArchiveService,
    ITimeControlTranslator timeControlTranslator,
    IRequiredActor<PlayerSessionActor> playerSessionActor,
    IUnitOfWork unitOfWork
) : IGameFinalizer
{
    private readonly UserManager<AuthedUser> _userManager = userManager;
    private readonly IRatingService _ratingService = ratingService;
    private readonly IGameArchiveService _gameArchiveService = gameArchiveService;
    private readonly ITimeControlTranslator _timeControlTranslator = timeControlTranslator;
    private readonly IRequiredActor<PlayerSessionActor> _playerSessionActor = playerSessionActor;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<GameArchive> FinalizeGameAsync(
        string gameToken,
        GameState state,
        GameResult result,
        string resultDescription,
        CancellationToken token = default
    )
    {
        var ratingDelta = await UpdateRatingAsync(state, result, token);
        var archive = await _gameArchiveService.CreateArchiveAsync(
            gameToken,
            state,
            result,
            resultDescription,
            ratingDelta,
            token
        );

        _playerSessionActor.ActorRef.Tell(new PlayerCommands.GameEnded(state.WhitePlayer.UserId));
        _playerSessionActor.ActorRef.Tell(new PlayerCommands.GameEnded(state.BlackPlayer.UserId));

        await _unitOfWork.CompleteAsync(token);

        return archive;
    }

    private async Task<RatingDelta> UpdateRatingAsync(
        GameState gameState,
        GameResult gameResult,
        CancellationToken token = default
    )
    {
        if (gameResult is GameResult.Aborted)
            return new();

        var whiteUser = await _userManager.FindByIdAsync(gameState.WhitePlayer.UserId);
        var blackUser = await _userManager.FindByIdAsync(gameState.BlackPlayer.UserId);
        if (whiteUser is null || blackUser is null)
            return new();

        var whiteRatingDelta = await _ratingService.UpdateRatingForResultAsync(
            whiteUser,
            blackUser,
            gameResult,
            _timeControlTranslator.FromSeconds(gameState.TimeControl.BaseSeconds),
            token
        );
        return whiteRatingDelta;
    }
}
