using Chess2.Api.ArchivedGames.Entities;
using Chess2.Api.ArchivedGames.Models;
using Chess2.Api.ArchivedGames.Repositories;
using Chess2.Api.GameSnapshot.Models;
using Chess2.Api.LiveGame.Errors;
using Chess2.Api.Shared.Models;
using Chess2.Api.UserRating.Models;
using ErrorOr;

namespace Chess2.Api.ArchivedGames.Services;

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
    IArchivedGameStateBuilder gameStateBuilder
) : IGameArchiveService
{
    private readonly IGameArchiveRepository _gameArchiveRepository = gameArchiveRepository;
    private readonly IArchivedGameStateBuilder _gameStateBuilder = gameStateBuilder;

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
        for (int i = 0; i < state.MoveHistory.Count; i++)
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
            InitialFen = state.InitialFen,
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

        var summeries = archives.Select(CreateGameSummary);
        return new(
            Items: summeries,
            TotalCount: totalCount,
            Page: pagination.Page,
            PageSize: pagination.PageSize
        );
    }

    private static GameSummaryDto CreateGameSummary(GameArchive archive) =>
        new(
            archive.GameToken,
            new PlayerSummaryDto(
                UserId: archive.WhitePlayer.UserId,
                UserName: archive.WhitePlayer.UserName,
                Rating: archive.WhitePlayer.NewRating
            ),
            new PlayerSummaryDto(
                UserId: archive.BlackPlayer.UserId,
                UserName: archive.BlackPlayer.UserName,
                Rating: archive.BlackPlayer.NewRating
            ),
            archive.Result,
            CreatedAt: archive.CreatedAt
        );

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

    private static MoveArchive CreateMoveArchive(MoveSnapshot moveSnapshot, int moveNumber)
    {
        var path = moveSnapshot.Path;
        var sideEffects =
            path.SideEffects?.Select(se => new MoveSideEffectArchive
                {
                    FromIdx = se.FromIdx,
                    ToIdx = se.ToIdx,
                })
                .ToList() ?? [];

        return new()
        {
            MoveNumber = moveNumber,
            San = moveSnapshot.San,
            TimeLeft = moveSnapshot.TimeLeft,
            FromIdx = path.FromIdx,
            ToIdx = path.ToIdx,
            Captures = path.CapturedIdxs?.ToList() ?? [],
            Triggers = path.TriggerIdxs?.ToList() ?? [],
            SideEffects = sideEffects,
        };
    }
}
