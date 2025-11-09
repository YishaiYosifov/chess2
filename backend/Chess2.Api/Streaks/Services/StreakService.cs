using Chess2.Api.Pagination.Models;
using Chess2.Api.Profile.DTOs;
using Chess2.Api.Profile.Models;
using Chess2.Api.Shared.Models;
using Chess2.Api.Shared.Services;
using Chess2.Api.Streaks.Models;
using Chess2.Api.Streaks.Repositories;

namespace Chess2.Api.Streaks.Services;

public interface IStreakService
{
    Task EndStreakAsync(UserId user, CancellationToken token = default);
    Task<int> GetHighestStreakAsync(UserId user, CancellationToken token = default);
    Task<PagedResult<StreakDto>> GetPaginatedLeaderboardAsync(
        PaginationQuery pagination,
        CancellationToken token = default
    );
    Task IncrementStreakAsync(UserId user, CancellationToken token = default);
}

public class StreakService(IStreakRepository repository, IUnitOfWork unitOfWork) : IStreakService
{
    private readonly IStreakRepository _repository = repository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<PagedResult<StreakDto>> GetPaginatedLeaderboardAsync(
        PaginationQuery pagination,
        CancellationToken token = default
    )
    {
        var streaks = await _repository.GetPaginatedLeaderboardAsync(pagination, token);
        var totalCount = await _repository.GetTotalCountAsync(token);

        return new(
            Items: streaks.Select(streak => new StreakDto(
                new MinimalProfile(streak.User),
                streak.HighestStreak
            )),
            TotalCount: totalCount,
            Page: pagination.Page,
            PageSize: pagination.PageSize
        );
    }

    public async Task<int> GetHighestStreakAsync(UserId user, CancellationToken token = default)
    {
        var streak = await _repository.GetUserStreakAsync(user, token);
        return streak is null ? 0 : streak.HighestStreak;
    }

    public async Task IncrementStreakAsync(UserId user, CancellationToken token = default)
    {
        var streak = await _repository.GetUserStreakAsync(user, token);
        if (streak is null)
        {
            streak = new()
            {
                UserId = user,
                CurrentStreak = 1,
                HighestStreak = 1,
            };
            await _repository.AddAsync(streak, token);
            await _unitOfWork.CompleteAsync(token);
            return;
        }

        streak.CurrentStreak++;
        streak.HighestStreak = Math.Max(streak.CurrentStreak, streak.HighestStreak);
        await _unitOfWork.CompleteAsync(token);
    }

    public async Task EndStreakAsync(UserId user, CancellationToken token = default)
    {
        await _repository.SetStreakAsync(user, streak: 0, token);
        await _unitOfWork.CompleteAsync(token);
    }
}
