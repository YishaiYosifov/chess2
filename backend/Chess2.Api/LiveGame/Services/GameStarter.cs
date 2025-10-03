using Chess2.Api.GameLogic.Models;
using Chess2.Api.GameSnapshot.Models;
using Chess2.Api.GameSnapshot.Services;
using Chess2.Api.LiveGame.Grains;
using Chess2.Api.Matchmaking.Models;
using Chess2.Api.Profile.Entities;
using Chess2.Api.Profile.Models;
using Chess2.Api.UserRating.Services;
using Microsoft.AspNetCore.Identity;

namespace Chess2.Api.LiveGame.Services;

public interface IGameStarter
{
    Task<string> StartGameAsync(UserId userId1, UserId userId2, PoolKey pool);
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

    public async Task<string> StartGameAsync(UserId userId1, UserId userId2, PoolKey pool)
    {
        var token = await _gameTokenGenerator.GenerateUniqueGameToken();
        // TODO: choose white and black based on each player last game
        var whitePlayer = await CreatePlayer(userId1, GameColor.White, pool.TimeControl);
        var blackPlayer = await CreatePlayer(userId2, GameColor.Black, pool.TimeControl);

        var gameGrain = _grains.GetGrain<IGameGrain>(token);
        await gameGrain.StartGameAsync(whitePlayer, blackPlayer, pool);

        return token;
    }

    private async Task<GamePlayer> CreatePlayer(
        UserId userId,
        GameColor color,
        TimeControlSettings timeControl
    )
    {
        var user = await _userManager.FindByIdAsync(userId);

        int? rating = user is null
            ? null
            : await _ratingService.GetRatingAsync(
                user,
                _timeControlTranslator.FromSeconds(timeControl.BaseSeconds)
            );

        return new GamePlayer(
            UserId: userId,
            IsAuthenticated: user is not null,
            Color: color,
            UserName: user?.UserName ?? "Guest",
            CountryCode: user?.CountryCode ?? "XX",
            Rating: rating
        );
    }
}
