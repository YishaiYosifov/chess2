﻿using Chess2.Api.Infrastructure;
using Chess2.Api.Pagination.Extensions;
using Chess2.Api.Pagination.Models;
using Chess2.Api.Quests.Entities;
using Microsoft.EntityFrameworkCore;

namespace Chess2.Api.Quests.Repositories;

public interface IQuestRepository
{
    Task AddQuestPointsAsync(UserQuestPoints questPoints, CancellationToken token = default);
    void DeleteAll();
    Task<List<UserQuestPoints>> GetPaginatedLeaderboardAsync(
        PaginationQuery pagination,
        CancellationToken token = default
    );
    Task<int> GetRankingAsync(int points, CancellationToken token = default);
    Task<int> GetTotalCountAsync(CancellationToken token = default);
    Task<UserQuestPoints?> GetUserPointsAsync(string userId, CancellationToken token = default);
}

public class QuestRepository(ApplicationDbContext dbContext) : IQuestRepository
{
    private readonly ApplicationDbContext _dbContext = dbContext;

    public Task<List<UserQuestPoints>> GetPaginatedLeaderboardAsync(
        PaginationQuery pagination,
        CancellationToken token = default
    ) =>
        _dbContext
            .QuestPoints.OrderByDescending(x => x.Points)
            .Paginate(pagination)
            .ToListAsync(token);

    public Task<int> GetTotalCountAsync(CancellationToken token = default) =>
        _dbContext.QuestPoints.CountAsync(token);

    public async Task<int> GetRankingAsync(int points, CancellationToken token = default) =>
        await _dbContext.QuestPoints.CountAsync(x => x.Points > points, token) + 1;

    public Task<UserQuestPoints?> GetUserPointsAsync(
        string userId,
        CancellationToken token = default
    ) => _dbContext.QuestPoints.FirstOrDefaultAsync(x => x.UserId == userId, token);

    public async Task AddQuestPointsAsync(
        UserQuestPoints questPoints,
        CancellationToken token = default
    ) => await _dbContext.AddAsync(questPoints, token);

    public void DeleteAll() => _dbContext.QuestPoints.ExecuteDelete();
}
