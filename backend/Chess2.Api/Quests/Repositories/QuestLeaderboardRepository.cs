using Chess2.Api.Infrastructure;
using Chess2.Api.Pagination.Extensions;
using Chess2.Api.Pagination.Models;
using Chess2.Api.Profile.Entities;
using Microsoft.EntityFrameworkCore;

namespace Chess2.Api.Quests.Repositories;

public interface IQuestLeaderboardRepository
{
    Task<List<AuthedUser>> GetPaginatedLeaderboardAsync(
        PaginationQuery pagination,
        CancellationToken token = default
    );
    Task<int> GetRankingAsync(AuthedUser user, CancellationToken token = default);
    Task<int> GetTotalCountAsync(CancellationToken token = default);
}

public class QuestLeaderboardRepository(ApplicationDbContext dbContext)
    : IQuestLeaderboardRepository
{
    private readonly ApplicationDbContext _dbContext = dbContext;

    public Task<List<AuthedUser>> GetPaginatedLeaderboardAsync(
        PaginationQuery pagination,
        CancellationToken token = default
    ) =>
        _dbContext
            .Users.Where(x => x.QuestPoints > 0)
            .OrderByDescending(x => x.QuestPoints)
            .Paginate(pagination)
            .ToListAsync(token);

    public Task<int> GetTotalCountAsync(CancellationToken token = default) =>
        _dbContext.Users.CountAsync(x => x.QuestPoints > 0, token);

    public async Task<int> GetRankingAsync(AuthedUser user, CancellationToken token = default) =>
        await _dbContext.Users.CountAsync(
            x => x.QuestPoints > 0 && x.QuestPoints > user.QuestPoints,
            token
        ) + 1;
}
