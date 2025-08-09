using Chess2.Api.GameLogic.Models;
using Chess2.Api.GameSnapshot.Models;
using Chess2.Api.GameSnapshot.Services;
using Chess2.Api.LiveGame.Actors;
using Chess2.Api.UserRating.Services;
using Chess2.Api.Users.Entities;
using Microsoft.AspNetCore.Identity;

namespace Chess2.Api.LiveGame.Services;

public interface IGameStarter
{
    Task<string> StartGameAsync(
        string userId1,
        string userId2,
        TimeControlSettings timeControl,
        bool isRated
    );
}

public class GameStarter(
    IGrainFactory grains,
    IGameTokenGenerator gameTokenGenerator,
    UserManager<AuthedUser> userManager,
    IRatingService ratingService,
    ITimeControlTranslator timeControlTranslator
) : IGameStarter
{
    private readonly IGameTokenGenerator _gameTokenGenerator = gameTokenGenerator;
    private readonly UserManager<AuthedUser> _userManager = userManager;
    private readonly IRatingService _ratingService = ratingService;
    private readonly ITimeControlTranslator _timeControlTranslator = timeControlTranslator;
    private readonly IGrainFactory _grains = grains;

    public async Task<string> StartGameAsync(
        string userId1,
        string userId2,
        TimeControlSettings timeControl,
        bool isRated
    )
    {
        var token = await _gameTokenGenerator.GenerateUniqueGameToken();
        // TODO: choose white and black based on each player last game
        var whitePlayer = await CreatePlayer(userId1, GameColor.White, timeControl);
        var blackPlayer = await CreatePlayer(userId2, GameColor.Black, timeControl);

        var gameGrain = _grains.GetGrain<IGameGrain>(token);
        await gameGrain.StartGameAsync(whitePlayer, blackPlayer, timeControl, isRated);

        return token;
    }

    private async Task<GamePlayer> CreatePlayer(
        string userId,
        GameColor color,
        TimeControlSettings timeControl
    )
    {
        var user = await _userManager.FindByIdAsync(userId);

        int? rating = null;
        if (user is not null)
            rating = await _ratingService.GetRatingAsync(
                user,
                _timeControlTranslator.FromSeconds(timeControl.BaseSeconds)
            );

        return new GamePlayer(
            UserId: userId,
            Color: color,
            UserName: user?.UserName ?? "Guest",
            CountryCode: user?.CountryCode,
            Rating: rating
        );
    }
}
