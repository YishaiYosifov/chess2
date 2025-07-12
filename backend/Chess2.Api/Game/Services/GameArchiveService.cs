using Chess2.Api.Game.Entities;
using Chess2.Api.Game.Errors;
using Chess2.Api.Game.Models;
using Chess2.Api.Game.Repositories;
using Chess2.Api.UserRating.Models;
using ErrorOr;

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
    Task<ErrorOr<GameState>> GetGameStateByTokenAsync(
        string gameToken,
        CancellationToken token = default
    );
}

public class GameArchiveService(
    IGameArchiveRepository gameArchiveRepository,
    IGameStateBuilder gameStateBuilder
) : IGameArchiveService
{
    private readonly IGameArchiveRepository _gameArchiveRepository = gameArchiveRepository;
    private readonly IGameStateBuilder _gameStateBuilder = gameStateBuilder;

    public async Task<GameArchive> CreateArchiveAsync(
        string gameToken,
        GameState state,
        GameResult result,
        string resultDescription,
        RatingDelta ratingDelta,
        CancellationToken token = default
    )
    {
        var whiteArchive = CreatePlayerArchive(
            state.WhitePlayer,
            ratingDelta.WhiteDelta,
            state.Clocks.WhiteClock
        );
        var blackArchive = CreatePlayerArchive(
            state.BlackPlayer,
            ratingDelta.BlackDelta,
            state.Clocks.BlackClock
        );
        List<MoveArchive> moves = [];
        for (int i = 0; i < state.MoveHistory.Count(); i++)
        {
            var moveArchive = CreateMoveArchive(state.MoveHistory.ElementAt(i), i);
            moves.Add(moveArchive);
        }

        GameArchive gameArchive = new()
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
            IsRated = state.IsRated,
            BaseSeconds = state.TimeControl.BaseSeconds,
            IncrementSeconds = state.TimeControl.IncrementSeconds,
        };

        await _gameArchiveRepository.AddArchiveAsync(gameArchive, token);
        return gameArchive;
    }

    public async Task<ErrorOr<GameState>> GetGameStateByTokenAsync(
        string gameToken,
        CancellationToken token = default
    )
    {
        var archive = await _gameArchiveRepository.GetGameArchiveByToken(gameToken, token);
        if (archive is null)
            return GameErrors.GameNotFound;

        var state = _gameStateBuilder.FromArchive(archive);
        return state;
    }

    private static PlayerArchive CreatePlayerArchive(
        GamePlayer player,
        int ratingDelta,
        double timeRemaining
    ) =>
        new()
        {
            Color = player.Color,
            UserId = player.UserId,
            UserName = player.UserName,
            FinalTimeRemaining = timeRemaining,
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
