using Chess2.Api.Game.Entities;
using Chess2.Api.Game.Models;
using Chess2.Api.Infrastructure;
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
        var whiteArchive = CreatePlayerArchive(gameState.WhitePlayer);
        var blackArchive = CreatePlayerArchive(gameState.BlackPlayer);
        List<MoveArchive> moves = [];
        for (int i = 0; i < gameState.MoveHistory.Count; i++)
        {
            var moveArchive = CreateMoveArchive(gameState.MoveHistory.ElementAt(i), i);
            moves.Add(moveArchive);
        }
        var gameArchive = new GameArchive()
        {
            GameToken = gameToken,
            Result = gameResult,
            WhitePlayerId = whiteArchive.Id,
            WhitePlayer = whiteArchive,
            BlackPlayerId = blackArchive.Id,
            BlackPlayer = blackArchive,
            FinalFen = gameState.Fen,
            Moves = moves,
        };

        _dbContext.GameArchives.Add(gameArchive);
        await _dbContext.SaveChangesAsync(token);
    }

    private PlayerArchive CreatePlayerArchive(GamePlayer player) =>
        new()
        {
            Color = player.Color,
            UserId = player.UserId,
            UserName = player.UserName,
            CountryCode = player.CountryCode,
            Rating = player.Rating,
        };

    private MoveArchive CreateMoveArchive(string encodedMove, int moveNumber) =>
        new() { MoveNumber = moveNumber, EncodedMove = encodedMove };
}
