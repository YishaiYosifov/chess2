using System.Linq.Expressions;
using Chess2.Api.Infrastructure;
using Chess2.Api.Pagination.Extensions;
using Chess2.Api.Pagination.Models;
using Chess2.Api.Quests.Entities;
using Microsoft.EntityFrameworkCore;

namespace Chess2.Api.Quests.Repositories;

public interface IQuestRepository
{
    Task AddQuestPointsAsync(UserQuestPoints questPoints, CancellationToken token = default);
    Task<List<UserQuestPoints>> GetPaginatedLeaderboardAsync(
        PaginationQuery pagination,
        DateTime asOfMonth,
        CancellationToken token = default
    );
    Task<int> GetRankingAsync(int points, DateTime asOfMonth, CancellationToken token = default);
    Task<int> GetTotalCountAsync(DateTime asOfMonth, CancellationToken token = default);
    Task<UserQuestPoints?> GetUserPointsAsync(
        string userId,
        DateTime asOfMonth,
        CancellationToken token = default
    );
}

public class QuestRepository(ApplicationDbContext dbContext) : IQuestRepository
{
    private readonly ApplicationDbContext _dbContext = dbContext;

    private static Expression<Func<UserQuestPoints, bool>> FilterByMonth(DateTime asOfMonth)
    {
        var start = asOfMonth;
        var end = start.AddMonths(1);

        return x => x.LastQuestAt >= start && x.LastQuestAt < end;
    }

    public Task<List<UserQuestPoints>> GetPaginatedLeaderboardAsync(
        PaginationQuery pagination,
        DateTime asOfMonth,
        CancellationToken token = default
    ) =>
        _dbContext
            .QuestPoints.Where(FilterByMonth(asOfMonth))
            .OrderByDescending(x => x.Points)
            .Paginate(pagination)
            .ToListAsync(token);

    public Task<int> GetTotalCountAsync(DateTime asOfMonth, CancellationToken token = default) =>
        _dbContext.QuestPoints.CountAsync(FilterByMonth(asOfMonth), token);

    public async Task<int> GetRankingAsync(
        int points,
        DateTime asOfMonth,
        CancellationToken token = default
    ) =>
        await _dbContext
            .QuestPoints.Where(FilterByMonth(asOfMonth))
            .CountAsync(x => x.Points > points, token) + 1;

    public Task<UserQuestPoints?> GetUserPointsAsync(
        string userId,
        DateTime asOfMonth,
        CancellationToken token = default
    ) =>
        _dbContext
            .QuestPoints.Where(FilterByMonth(asOfMonth))
            .FirstOrDefaultAsync(x => x.UserId == userId, token);

    public async Task AddQuestPointsAsync(
        UserQuestPoints questPoints,
        CancellationToken token = default
    ) => await _dbContext.AddAsync(questPoints, token);
}
