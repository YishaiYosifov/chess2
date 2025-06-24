using Chess2.Api.Game.Entities;
using Chess2.Api.Game.Models;
using Chess2.Api.Infrastructure;
using Chess2.Api.UserRating.Services;
using Chess2.Api.Users.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

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
    ApplicationDbContext dbContext
) : IGameFinalizer
{
    private readonly UserManager<AuthedUser> _userManager = userManager;
    private readonly IRatingService _ratingService = ratingService;
    private readonly ApplicationDbContext _dbContext = dbContext;

    public async Task FinalizeGameAsync(
        string gameToken,
        GameState gameState,
        GameResult gameResult,
        CancellationToken token = default
    )
    {
        var test = await _dbContext.MoveArchives.ToListAsync(token);
        var whiteArchive = CreatePlayerArchive(gameState.WhitePlayer);
        var blackArchive = CreatePlayerArchive(gameState.BlackPlayer);
        var gameArchive = new GameArchive()
        {
            GameToken = gameToken,
            Result = gameResult,
            WhitePlayer = whiteArchive,
            BlackPlayer = blackArchive,
            FinalFen = gameState.Fen,
        };

        for (int i = 0; i < gameState.MoveHistory.Count; i++)
        {
            var moveArchive = CreateMoveArchive(gameState.MoveHistory.ElementAt(i), gameArchive, i);
            _dbContext.MoveArchives.Add(moveArchive);
        }

        _dbContext.GameArchives.Add(gameArchive);
        await _dbContext.SaveChangesAsync(token);
    }

    private PlayerArchive CreatePlayerArchive(GamePlayer player) =>
        new() { Color = player.Color, UserId = player.UserId };

    private MoveArchive CreateMoveArchive(string encodedMove, GameArchive game, int moveNumber) =>
        new()
        {
            MoveNumber = moveNumber,
            EncodedMove = encodedMove,
            Game = game,
        };
}
