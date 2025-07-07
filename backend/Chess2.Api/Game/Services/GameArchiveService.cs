using Chess2.Api.Game.Entities;
using Chess2.Api.Game.Models;
using Chess2.Api.Game.Repositories;
using Chess2.Api.UserRating.Models;

namespace Chess2.Api.Game.Services;

public interface IGameArchiveService
{
    Task<GameArchive> CreateArchiveAsync(
        string gameToken,
        GameState state,
        GameResult result,
        string resultDescription,
        RatingDelta ratingDelta,
        CancellationToken token = default
    );
}

public class GameArchiveService(IGameArchiveRepository gameArchiveRepository) : IGameArchiveService
{
    private readonly IGameArchiveRepository _gameArchiveRepository = gameArchiveRepository;

    public async Task<GameArchive> CreateArchiveAsync(
        string gameToken,
        GameState state,
        GameResult result,
        string resultDescription,
        RatingDelta ratingDelta,
        CancellationToken token = default
    )
    {
        var whiteArchive = CreatePlayerArchive(state.WhitePlayer, ratingDelta.WhiteDelta);
        var blackArchive = CreatePlayerArchive(state.BlackPlayer, ratingDelta.BlackDelta);
        List<MoveArchive> moves = [];
        for (int i = 0; i < state.MoveHistory.Count(); i++)
        {
            var moveArchive = CreateMoveArchive(state.MoveHistory.ElementAt(i), i);
            moves.Add(moveArchive);
        }

        var gameArchive = new GameArchive()
        {
            GameToken = gameToken,
            Result = result,
            ResultDescription = resultDescription,
            WhitePlayerId = whiteArchive.Id,
            WhitePlayer = whiteArchive,
            BlackPlayerId = blackArchive.Id,
            BlackPlayer = blackArchive,
            FinalFen = state.Fen,
            Moves = moves,
        };

        await _gameArchiveRepository.AddArchiveAsync(gameArchive, token);
        return gameArchive;
    }

    private static PlayerArchive CreatePlayerArchive(GamePlayer player, int ratingDelta) =>
        new()
        {
            Color = player.Color,
            UserId = player.UserId,
            UserName = player.UserName,
            CountryCode = player.CountryCode,
            InitialRating = player.Rating,
            NewRating = player.Rating + ratingDelta,
        };

    private static MoveArchive CreateMoveArchive(MoveSnapshot moveSnapshot, int moveNumber) =>
        new()
        {
            MoveNumber = moveNumber,
            EncodedMove = moveSnapshot.EncodedMove,
            San = moveSnapshot.San,
            TimeLeft = moveSnapshot.TimeLeft,
        };
}
