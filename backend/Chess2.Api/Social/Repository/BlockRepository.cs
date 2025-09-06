using Chess2.Api.Infrastructure;
using Chess2.Api.Pagination.Extensions;
using Chess2.Api.Pagination.Models;
using Chess2.Api.Profile.Entities;
using Chess2.Api.Social.Entities;
using Microsoft.EntityFrameworkCore;

namespace Chess2.Api.Social.Repository;

public interface IBlockRepository
{
    Task AddBlockedUserAsync(BlockedUser blockedUser, CancellationToken token = default);
    Task<int> GetBlockedCountAsync(string userId, CancellationToken token = default);
    Task<BlockedUser?> GetBlockedUserAsync(
        string blockedByUserId,
        string blockedUserId,
        CancellationToken token = default
    );
    Task<List<AuthedUser>> GetPaginatedBlockedUsersAsync(
        string userId,
        PaginationQuery query,
        CancellationToken token = default
    );
    void RemoveBlockedUser(BlockedUser blockedUser);
}

public class BlockRepository(ApplicationDbContext dbContext) : IBlockRepository
{
    private readonly ApplicationDbContext _dbContext = dbContext;

    public Task<List<AuthedUser>> GetPaginatedBlockedUsersAsync(
        string userId,
        PaginationQuery query,
        CancellationToken token = default
    ) =>
        _dbContext
            .BlockedUsers.Where(x => x.UserId == userId)
            .Select(x => x.Blocked)
            .Paginate(query)
            .ToListAsync(token);

    public Task<int> GetBlockedCountAsync(string userId, CancellationToken token = default) =>
        _dbContext.BlockedUsers.Where(x => x.UserId == userId).CountAsync(token);

    public Task<BlockedUser?> GetBlockedUserAsync(
        string blockedByUserId,
        string blockedUserId,
        CancellationToken token = default
    ) =>
        _dbContext
            .BlockedUsers.Where(x =>
                x.UserId == blockedByUserId && x.BlockedUserId == blockedUserId
            )
            .FirstOrDefaultAsync(token);

    public async Task AddBlockedUserAsync(
        BlockedUser blockedUser,
        CancellationToken token = default
    ) => await _dbContext.BlockedUsers.AddAsync(blockedUser, token);

    public void RemoveBlockedUser(BlockedUser blockedUser) =>
        _dbContext.BlockedUsers.Remove(blockedUser);
}
