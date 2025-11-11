using Chess2.Api.Game.Models;
using Chess2.Api.Pagination.Models;
using Chess2.Api.Profile.Entities;
using Chess2.Api.Profile.Models;
using Chess2.Api.Shared.Models;
using Chess2.Api.Shared.Services;
using Chess2.Api.Streaks.Entities;
using Chess2.Api.Streaks.Models;
using Chess2.Api.Streaks.Repositories;

namespace Chess2.Api.Streaks.Services;

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
        var rank = await _repository.GetRankingAsync(streak?.HighestStreak ?? 0, token);
        return new(
            Rank: rank,
            HighestStreak: streak?.HighestStreak ?? 0,
            CurrentStreak: streak?.CurrentStreak ?? 0
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

        streak.CurrentStreak++;
        streak.CurrentStreakGameTokens.Add(gameWon);

        if (streak.CurrentStreak > streak.HighestStreak)
        {
            streak.HighestStreak = streak.CurrentStreak;
            streak.HighestStreakGameTokens = streak.CurrentStreakGameTokens;
        }
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
            CurrentStreak = 1,
            CurrentStreakGameTokens = [gameWon],
            HighestStreak = 1,
            HighestStreakGameTokens = [gameWon],
        };
        await _repository.AddAsync(streak, token);
    }
}
