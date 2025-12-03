using AnarchyChess.Api.Game.Grains;
using AnarchyChess.Api.Game.Models;
using AnarchyChess.Api.GameLogic.Models;
using AnarchyChess.Api.GameSnapshot.Models;
using AnarchyChess.Api.Matchmaking.Models;
using AnarchyChess.Api.Profile.Entities;
using AnarchyChess.Api.Profile.Models;
using AnarchyChess.Api.Shared.Services;
using AnarchyChess.Api.UserRating.Services;
using Microsoft.AspNetCore.Identity;

namespace AnarchyChess.Api.Game.Services;

public interface IGameStarter
{
    Task<GameToken> StartGameWithColorsAsync(
        UserId whiteUserId,
        UserId blackUserId,
        PoolKey pool,
        GameSource gameSource = GameSource.Unknown,
        CancellationToken token = default
    );
    Task<GameToken> StartGameWithRandomColorsAsync(
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
    IRandomCodeGenerator randomCodeGenerator,
    IRandomProvider randomProvider
) : IGameStarter
{
    private readonly UserManager<AuthedUser> _userManager = userManager;
    private readonly IRatingService _ratingService = ratingService;
    private readonly IRandomCodeGenerator _randomCodeGenerator = randomCodeGenerator;
    private readonly IRandomProvider _randomProvider = randomProvider;
    private readonly IGrainFactory _grains = grains;

    public async Task<GameToken> StartGameWithRandomColorsAsync(
        UserId userId1,
        UserId userId2,
        PoolKey pool,
        GameSource gameSource = GameSource.Unknown,
        CancellationToken token = default
    )
    {
        var isUser1White = _randomProvider.Next(2) == 0;
        var whiteUserId = isUser1White ? userId1 : userId2;
        var blackUserId = isUser1White ? userId2 : userId1;

        var gameToken = await StartGameWithColorsAsync(
            whiteUserId,
            blackUserId,
            pool,
            gameSource,
            token
        );
        return gameToken;
    }

    public async Task<GameToken> StartGameWithColorsAsync(
        UserId whiteUserId,
        UserId blackUserId,
        PoolKey pool,
        GameSource gameSource = GameSource.Unknown,
        CancellationToken token = default
    )
    {
        GameToken gameToken = _randomCodeGenerator.Generate(16);

        var whitePlayer = await CreatePlayer(whiteUserId, GameColor.White, pool.TimeControl);
        var blackPlayer = await CreatePlayer(blackUserId, GameColor.Black, pool.TimeControl);

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
            : await _ratingService.GetRatingAsync(user.Id, timeControl.Type);

        return new GamePlayer(
            UserId: userId,
            Color: color,
            UserName: user?.UserName ?? "Guest",
            CountryCode: user?.CountryCode ?? "XX",
            Rating: rating
        );
    }
}
