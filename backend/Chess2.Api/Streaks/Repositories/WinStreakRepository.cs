using Chess2.Api.Infrastructure;
using Chess2.Api.Pagination.Extensions;
using Chess2.Api.Pagination.Models;
using Chess2.Api.Profile.Models;
using Chess2.Api.Streaks.Entities;
using Microsoft.EntityFrameworkCore;

namespace Chess2.Api.Streaks.Repositories;

public interface IWinStreakRepository
{
    Task AddAsync(UserWinStreak streak, CancellationToken token = default);
    Task<List<UserWinStreak>> GetPaginatedLeaderboardAsync(
        PaginationQuery pagination,
        CancellationToken token = default
    );
    Task<int> GetRankingAsync(int highestStreak, CancellationToken token = default);
    Task<int> GetTotalCountAsync(CancellationToken token = default);
    Task<UserWinStreak?> GetUserStreakAsync(UserId userId, CancellationToken token = default);
    Task ClearCurrentStreakAsync(UserId userId, CancellationToken token = default);
}

public class WinStreakRepository(ApplicationDbContext dbContext) : IWinStreakRepository
{
    private readonly ApplicationDbContext _dbContext = dbContext;

    public async Task AddAsync(UserWinStreak streak, CancellationToken token = default) =>
        await _dbContext.WinStreaks.AddAsync(streak, token);

    public Task ClearCurrentStreakAsync(UserId userId, CancellationToken token = default) =>
        _dbContext
            .WinStreaks.Where(x => x.UserId == userId)
            .ExecuteUpdateAsync(
                setters => setters.SetProperty(x => x.CurrentStreakGames, new List<string>()),
                token
            );

    public Task<UserWinStreak?> GetUserStreakAsync(
        UserId userId,
        CancellationToken token = default
    ) => _dbContext.WinStreaks.Where(x => x.UserId == userId).FirstOrDefaultAsync(token);

    public Task<List<UserWinStreak>> GetPaginatedLeaderboardAsync(
        PaginationQuery pagination,
        CancellationToken token = default
    ) =>
        _dbContext
            .WinStreaks.OrderByDescending(x => x.HighestStreakGames.Count)
            .Paginate(pagination)
            .ToListAsync(token);

    public Task<int> GetTotalCountAsync(CancellationToken token = default) =>
        _dbContext.WinStreaks.CountAsync(token);

    public async Task<int> GetRankingAsync(int highestStreak, CancellationToken token = default) =>
        await _dbContext.WinStreaks.CountAsync(
            x => x.HighestStreakGames.Count > highestStreak,
            token
        ) + 1;
}
