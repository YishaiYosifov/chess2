using AnarchyChess.Api.Infrastructure;
using AnarchyChess.Api.Pagination.Extensions;
using AnarchyChess.Api.Pagination.Models;
using AnarchyChess.Api.Profile.Entities;
using AnarchyChess.Api.Profile.Models;
using AnarchyChess.Api.Social.Entities;
using Microsoft.EntityFrameworkCore;

namespace AnarchyChess.Api.Social.Repository;

public interface IBlockRepository
{
    Task AddBlockedUserAsync(BlockedUser blockedUser, CancellationToken token = default);
    Task<HashSet<UserId>> GetAllBlockedUserIdsAsync(
        UserId userId,
        CancellationToken token = default
    );
    Task<int> GetBlockedCountAsync(UserId userId, CancellationToken token = default);
    Task<BlockedUser?> GetBlockedUserAsync(
        UserId blockedByUserId,
        UserId blockedUserId,
        CancellationToken token = default
    );
    Task<List<AuthedUser>> GetPaginatedBlockedUsersAsync(
        UserId userId,
        PaginationQuery query,
        CancellationToken token = default
    );
    void RemoveBlockedUser(BlockedUser blockedUser);
}

public class BlockRepository(ApplicationDbContext dbContext) : IBlockRepository
{
    private readonly ApplicationDbContext _dbContext = dbContext;

    public Task<List<AuthedUser>> GetPaginatedBlockedUsersAsync(
        UserId userId,
        PaginationQuery query,
        CancellationToken token = default
    ) =>
        _dbContext
            .BlockedUsers.Where(x => x.UserId == userId)
            .Select(x => x.Blocked)
            .Paginate(query)
            .ToListAsync(token);

    public Task<HashSet<UserId>> GetAllBlockedUserIdsAsync(
        UserId userId,
        CancellationToken token = default
    ) =>
        _dbContext
            .BlockedUsers.IgnoreAutoIncludes()
            .Where(x => x.UserId == userId)
            .Select(x => x.BlockedUserId)
            .ToHashSetAsync(token);

    public Task<int> GetBlockedCountAsync(UserId userId, CancellationToken token = default) =>
        _dbContext.BlockedUsers.Where(x => x.UserId == userId).CountAsync(token);

    public Task<BlockedUser?> GetBlockedUserAsync(
        UserId blockedByUserId,
        UserId blockedUserId,
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
