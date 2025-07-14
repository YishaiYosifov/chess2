using Chess2.Api.Game.Entities;
using Chess2.Api.Game.Errors;
using Chess2.Api.Game.Models;
using Chess2.Api.Game.Repositories;
using Chess2.Api.Shared.Models;
using Chess2.Api.UserRating.Models;
using ErrorOr;

namespace Chess2.Api.Game.Services;

public interface IGameArchiveService
{
    Task<GameArchive> CreateArchiveAsync(
        string gameToken,
        GameState state,
        GameEndStatus endStatus,
        RatingChange? ratingChange,
        CancellationToken token = default
    );
    Task<ErrorOr<GameState>> GetGameStateByTokenAsync(
        string gameToken,
        CancellationToken token = default
    );
    Task<PagedResult<GameSummaryDto>> GetPaginatedResultsAsync(
        string userId,
        PaginationQuery pagination,
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
        GameEndStatus endStatus,
        RatingChange? ratingChange,
        CancellationToken token = default
    )
    {
        var whiteArchive = CreatePlayerArchive(
            state.WhitePlayer,
            ratingChange?.WhiteChange,
            state.Clocks.WhiteClock
        );
        var blackArchive = CreatePlayerArchive(
            state.BlackPlayer,
            ratingChange?.BlackChange,
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
            Result = endStatus.Result,
            ResultDescription = endStatus.ResultDescription,
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
        var archive = await _gameArchiveRepository.GetGameArchiveByTokenAsync(gameToken, token);
        if (archive is null)
            return GameErrors.GameNotFound;

        var state = _gameStateBuilder.FromArchive(archive);
        return state;
    }

    public async Task<PagedResult<GameSummaryDto>> GetPaginatedResultsAsync(
        string userId,
        PaginationQuery pagination,
        CancellationToken token = default
    )
    {
        var archives = await _gameArchiveRepository.GetPaginatedArchivedGamesForUserAsync(
            userId,
            take: pagination.PageSize,
            skip: pagination.Skip,
            token
        );
        var totalCount = await _gameArchiveRepository.CountArchivedGamesForUserAsync(userId, token);

        var summeries = archives.Select(archive =>
        {
            var whitePlayer = archive.WhitePlayer;
            var blackPlayer = archive.BlackPlayer;
            return new GameSummaryDto(
                archive.GameToken,
                new PlayerSummaryDto(
                    UserId: whitePlayer.UserId,
                    UserName: whitePlayer.UserName,
                    Rating: whitePlayer.NewRating
                ),
                new PlayerSummaryDto(
                    UserId: blackPlayer.UserId,
                    UserName: blackPlayer.UserName,
                    Rating: blackPlayer.NewRating
                ),
                archive.Result,
                CreatedAt: archive.CreatedAt
            );
        });

        return new(
            Items: summeries,
            TotalCount: totalCount,
            Page: pagination.Page,
            PageSize: pagination.PageSize
        );
    }

    private static PlayerArchive CreatePlayerArchive(
        GamePlayer player,
        int? ratingChange,
        double timeRemaining
    ) =>
        new()
        {
            Color = player.Color,
            UserId = player.UserId,
            UserName = player.UserName,
            FinalTimeRemaining = timeRemaining,
            CountryCode = player.CountryCode,
            NewRating = player.Rating + (ratingChange ?? 0),
            RatingChange = ratingChange,
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
