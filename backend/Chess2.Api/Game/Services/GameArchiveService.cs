using Chess2.Api.Game.Entities;
using Chess2.Api.Game.Models;
using Chess2.Api.Game.Repositories;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Chess2.Api.Game.Services;

public interface IGameArchiveService
{
    Task<GameArchive> CreateArchiveAsync(
        string gameToken,
        GameState gameState,
        GameResult gameResult,
        CancellationToken token = default
    );
}

public class GameArchiveService(IGameArchiveRepository gameArchiveRepository) : IGameArchiveService
{
    private readonly IGameArchiveRepository _gameArchiveRepository = gameArchiveRepository;

    public async Task<GameArchive> CreateArchiveAsync(
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

        await _gameArchiveRepository.AddArchiveAsync(gameArchive, token);
        return gameArchive;
    }

    private static PlayerArchive CreatePlayerArchive(GamePlayer player) =>
        new()
        {
            Color = player.Color,
            UserId = player.UserId,
            UserName = player.UserName,
            CountryCode = player.CountryCode,
            Rating = player.Rating,
        };

    private static MoveArchive CreateMoveArchive(string encodedMove, int moveNumber) =>
        new() { MoveNumber = moveNumber, EncodedMove = encodedMove };
}
