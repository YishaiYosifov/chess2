using Chess2.Api.Infrastructure;
using Chess2.Api.Pagination.Extensions;
using Chess2.Api.Pagination.Models;
using Chess2.Api.Profile.Models;
using Chess2.Api.Streaks.Entities;
using Microsoft.EntityFrameworkCore;

namespace Chess2.Api.Streaks.Repositories;

public interface IStreakRepository
{
    Task AddAsync(UserStreak streak, CancellationToken token = default);
    Task<List<UserStreak>> GetPaginatedLeaderboardAsync(
        PaginationQuery pagination,
        CancellationToken token = default
    );
    Task<int> GetRankingAsync(int highestStreak, CancellationToken token = default);
    Task<int> GetTotalCountAsync(CancellationToken token = default);
    Task<UserStreak?> GetUserStreakAsync(UserId userId, CancellationToken token = default);
    Task ClearCurrentStreakAsync(UserId userId, CancellationToken token = default);
}

public class StreakRepository(ApplicationDbContext dbContext) : IStreakRepository
{
    private readonly ApplicationDbContext _dbContext = dbContext;

    public async Task AddAsync(UserStreak streak, CancellationToken token = default) =>
        await _dbContext.Streaks.AddAsync(streak, token);

    public Task ClearCurrentStreakAsync(UserId userId, CancellationToken token = default) =>
        _dbContext
            .Streaks.Where(x => x.UserId == userId)
            .ExecuteUpdateAsync(
                setters =>
                    setters
                        .SetProperty(x => x.CurrentStreak, 0)
                        .SetProperty(x => x.CurrentStreakGames, new List<string>()),
                token
            );

    public Task<UserStreak?> GetUserStreakAsync(UserId userId, CancellationToken token = default) =>
        _dbContext.Streaks.Where(x => x.UserId == userId).FirstOrDefaultAsync(token);

    public Task<List<UserStreak>> GetPaginatedLeaderboardAsync(
        PaginationQuery pagination,
        CancellationToken token = default
    ) =>
        _dbContext
            .Streaks.OrderByDescending(x => x.HighestStreak)
            .Paginate(pagination)
            .ToListAsync(token);

    public Task<int> GetTotalCountAsync(CancellationToken token = default) =>
        _dbContext.Streaks.CountAsync(token);

    public async Task<int> GetRankingAsync(int highestStreak, CancellationToken token = default) =>
        await _dbContext.Streaks.CountAsync(x => x.HighestStreak > highestStreak, token) + 1;
}
