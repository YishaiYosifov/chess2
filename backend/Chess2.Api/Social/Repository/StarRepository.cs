using Chess2.Api.Infrastructure;
using Chess2.Api.Pagination.Extensions;
using Chess2.Api.Pagination.Models;
using Chess2.Api.Profile.Entities;
using Chess2.Api.Profile.Models;
using Chess2.Api.Social.Entities;
using Microsoft.EntityFrameworkCore;

namespace Chess2.Api.Social.Repository;

public interface IStarRepository
{
    Task AddStarAsync(StarredUser starredUser, CancellationToken token = default);
    Task<StarredUser?> GetStarAsync(
        UserId userId,
        UserId starredUserId,
        CancellationToken token = default
    );
    Task<List<AuthedUser>> GetPaginatedStarsGivenAsync(
        UserId userId,
        PaginationQuery query,
        CancellationToken token = default
    );
    void RemoveStar(StarredUser starredUser);
    Task<int> GetStarsGivenCount(UserId userId, CancellationToken token = default);
    Task<int> GetStarsReceivedCountAsync(UserId userId, CancellationToken token = default);
}

public class StarRepository(ApplicationDbContext dbContext) : IStarRepository
{
    private readonly ApplicationDbContext _dbContext = dbContext;

    public Task<List<AuthedUser>> GetPaginatedStarsGivenAsync(
        UserId userId,
        PaginationQuery query,
        CancellationToken token = default
    ) =>
        _dbContext
            .StarredUsers.Where(x => x.UserId == userId)
            .Select(x => x.Starred)
            .Paginate(query)
            .ToListAsync(token);

    public Task<int> GetStarsGivenCount(UserId userId, CancellationToken token = default) =>
        _dbContext.StarredUsers.Where(x => x.UserId == userId).CountAsync(token);

    public Task<int> GetStarsReceivedCountAsync(UserId userId, CancellationToken token = default) =>
        _dbContext.StarredUsers.Where(x => x.StarredUserId == userId).CountAsync(token);

    public Task<StarredUser?> GetStarAsync(
        UserId userId,
        UserId starredUserId,
        CancellationToken token = default
    ) =>
        _dbContext
            .StarredUsers.Where(x => x.UserId == userId && x.StarredUserId == starredUserId)
            .FirstOrDefaultAsync(token);

    public async Task AddStarAsync(StarredUser starredUser, CancellationToken token = default) =>
        await _dbContext.StarredUsers.AddAsync(starredUser, token);

    public void RemoveStar(StarredUser starredUser) => _dbContext.StarredUsers.Remove(starredUser);
}
