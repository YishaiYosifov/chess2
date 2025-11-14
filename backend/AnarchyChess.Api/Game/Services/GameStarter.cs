using AnarchyChess.Api.Game.Grains;
using AnarchyChess.Api.Game.Models;
using AnarchyChess.Api.GameLogic.Models;
using AnarchyChess.Api.GameSnapshot.Models;
using AnarchyChess.Api.GameSnapshot.Services;
using AnarchyChess.Api.Matchmaking.Models;
using AnarchyChess.Api.Profile.Entities;
using AnarchyChess.Api.Profile.Models;
using AnarchyChess.Api.Shared.Services;
using AnarchyChess.Api.UserRating.Services;
using Microsoft.AspNetCore.Identity;

namespace AnarchyChess.Api.Game.Services;

public interface IGameStarter
{
    Task<GameToken> StartGameAsync(
        UserId userId1,
        UserId userId2,
        PoolKey pool,
        GameSource gameSource = GameSource.Unknown,
        CancellationToken token = default
    );
}

public class GameStarter(
    IGrainFactory grains,
    UserManager<AuthedUser> userManager,
    IRatingService ratingService,
    ITimeControlTranslator timeControlTranslator,
    IRandomCodeGenerator randomCodeGenerator
) : IGameStarter
{
    private readonly UserManager<AuthedUser> _userManager = userManager;
    private readonly IRatingService _ratingService = ratingService;
    private readonly ITimeControlTranslator _timeControlTranslator = timeControlTranslator;
    private readonly IRandomCodeGenerator _randomCodeGenerator = randomCodeGenerator;
    private readonly IGrainFactory _grains = grains;

    public async Task<GameToken> StartGameAsync(
        UserId userId1,
        UserId userId2,
        PoolKey pool,
        GameSource gameSource = GameSource.Unknown,
        CancellationToken token = default
    )
    {
        GameToken gameToken = _randomCodeGenerator.Generate(16);

        // TODO: choose white and black based on each player last game
        var whitePlayer = await CreatePlayer(userId1, GameColor.White, pool.TimeControl);
        var blackPlayer = await CreatePlayer(userId2, GameColor.Black, pool.TimeControl);

        var gameGrain = _grains.GetGrain<IGameGrain>(gameToken);
        await gameGrain.StartGameAsync(whitePlayer, blackPlayer, pool, gameSource, token);

        return gameToken;
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
                user.Id,
                _timeControlTranslator.FromSeconds(timeControl.BaseSeconds)
            );

        return new GamePlayer(
            UserId: userId,
            Color: color,
            UserName: user?.UserName ?? "Guest",
            CountryCode: user?.CountryCode ?? "XX",
            Rating: rating
        );
    }
}
