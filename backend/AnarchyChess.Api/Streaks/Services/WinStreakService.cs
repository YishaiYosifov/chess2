using AnarchyChess.Api.Game.Models;
using AnarchyChess.Api.Pagination.Models;
using AnarchyChess.Api.Profile.Entities;
using AnarchyChess.Api.Profile.Models;
using AnarchyChess.Api.Shared.Models;
using AnarchyChess.Api.Shared.Services;
using AnarchyChess.Api.Streaks.Entities;
using AnarchyChess.Api.Streaks.Models;
using AnarchyChess.Api.Streaks.Repositories;

namespace AnarchyChess.Api.Streaks.Services;

public interface IWinStreakService
{
    Task EndStreakAsync(UserId userId, CancellationToken token = default);
    Task<PagedResult<WinStreakDto>> GetPaginatedLeaderboardAsync(
        PaginationQuery pagination,
        CancellationToken token = default
    );
    Task<MyWinStreakStats> GetMyStatsAsync(UserId userId, CancellationToken token = default);
    Task IncrementStreakAsync(
        AuthedUser user,
        GameToken gameWon,
        CancellationToken token = default
    );
}

public class WinStreakService(IWinStreakRepository repository, IUnitOfWork unitOfWork)
    : IWinStreakService
{
    private readonly IWinStreakRepository _repository = repository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<PagedResult<WinStreakDto>> GetPaginatedLeaderboardAsync(
        PaginationQuery pagination,
        CancellationToken token = default
    )
    {
        var streaks = await _repository.GetPaginatedLeaderboardAsync(pagination, token);
        var totalCount = await _repository.GetTotalCountAsync(token);

        return new(
            Items: streaks.Select(streak => new WinStreakDto(streak)),
            TotalCount: totalCount,
            Page: pagination.Page,
            PageSize: pagination.PageSize
        );
    }

    public async Task<MyWinStreakStats> GetMyStatsAsync(
        UserId userId,
        CancellationToken token = default
    )
    {
        var streak = await _repository.GetUserStreakAsync(userId, token);
        var rank = await _repository.GetRankingAsync(streak?.HighestStreakGames.Count ?? 0, token);
        return new(
            Rank: rank,
            HighestStreak: streak?.HighestStreakGames.Count ?? 0,
            CurrentStreak: streak?.CurrentStreakGames.Count ?? 0
        );
    }

    public async Task IncrementStreakAsync(
        AuthedUser user,
        GameToken gameWon,
        CancellationToken token = default
    )
    {
        var streak = await _repository.GetUserStreakAsync(user.Id, token);
        if (streak is null)
        {
            await StartNewStreakAsync(user, gameWon, token);
            await _unitOfWork.CompleteAsync(token);
            return;
        }

        streak.CurrentStreakGames.Add(gameWon);
        if (streak.CurrentStreakGames.Count > streak.HighestStreakGames.Count)
            streak.HighestStreakGames = streak.CurrentStreakGames;

        await _unitOfWork.CompleteAsync(token);
    }

    public async Task EndStreakAsync(UserId userId, CancellationToken token = default)
    {
        await _repository.ClearCurrentStreakAsync(userId, token);
        await _unitOfWork.CompleteAsync(token);
    }

    private async Task StartNewStreakAsync(
        AuthedUser user,
        GameToken gameWon,
        CancellationToken token = default
    )
    {
        UserWinStreak streak = new()
        {
            UserId = user.Id,
            User = user,
            CurrentStreakGames = [gameWon],
            HighestStreakGames = [gameWon],
        };
        await _repository.AddAsync(streak, token);
    }
}
