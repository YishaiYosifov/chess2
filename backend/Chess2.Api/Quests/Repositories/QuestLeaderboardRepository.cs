using Chess2.Api.Infrastructure;
using Chess2.Api.Profile.Entities;
using Microsoft.EntityFrameworkCore;

namespace Chess2.Api.Quests.Repositories;

public interface IQuestLeaderboardRepository
{
    Task<List<AuthedUser>> GetTopQuestPointsAsync(int top, CancellationToken token = default);
    Task<int> GetUserRankingAsync(AuthedUser user, CancellationToken token = default);
}

public class QuestLeaderboardRepository(ApplicationDbContext dbContext)
    : IQuestLeaderboardRepository
{
    private readonly ApplicationDbContext _dbContext = dbContext;

    public Task<List<AuthedUser>> GetTopQuestPointsAsync(
        int top,
        CancellationToken token = default
    ) => _dbContext.Users.OrderByDescending(x => x.QuestPoints).Take(top).ToListAsync(token);

    public async Task<int> GetUserRankingAsync(
        AuthedUser user,
        CancellationToken token = default
    ) => await _dbContext.Users.CountAsync(u => u.QuestPoints > user.QuestPoints, token) + 1;
}
