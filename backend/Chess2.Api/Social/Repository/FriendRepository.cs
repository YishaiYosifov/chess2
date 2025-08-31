using Chess2.Api.Infrastructure;
using Chess2.Api.Pagination.Extensions;
using Chess2.Api.Pagination.Models;
using Chess2.Api.Profile.Entities;
using Chess2.Api.Social.Entities;
using Microsoft.EntityFrameworkCore;

namespace Chess2.Api.Social.Repository;

public interface IFriendRepository
{
    Task AddFriendAsync(Friend friend, CancellationToken token = default);
    Task AddFriendRequestAsync(FriendRequest request, CancellationToken token = default);
    void DeleteFriendRequest(FriendRequest request);
    Task<List<AuthedUser>> GetIncomingFriendRequestsAsync(
        string userId,
        PaginationQuery query,
        CancellationToken token = default
    );
    Task<FriendRequest?> GetRequestBetweenAsync(
        string userId1,
        string userId2,
        CancellationToken token = default
    );
    Task<int> GetIncomingFriendRequestCount(string userId, CancellationToken token = default);
    Task<Friend?> GetFriendBetweenAsync(
        string userId1,
        string userId2,
        CancellationToken token = default
    );
}

public class FriendRepository(ApplicationDbContext dbContext) : IFriendRepository
{
    private readonly ApplicationDbContext _dbContext = dbContext;

    public Task<List<AuthedUser>> GetIncomingFriendRequestsAsync(
        string userId,
        PaginationQuery query,
        CancellationToken token = default
    ) =>
        _dbContext
            .FriendRequests.IgnoreAutoIncludes()
            .Include(x => x.Requester)
            .Where(x => x.RecipientUserId == userId)
            .Select(x => x.Requester)
            .Paginate(query)
            .ToListAsync(token);

    public Task<int> GetIncomingFriendRequestCount(
        string userId,
        CancellationToken token = default
    ) => _dbContext.FriendRequests.Where(x => x.RecipientUserId == userId).CountAsync(token);

    public Task<FriendRequest?> GetRequestBetweenAsync(
        string userId1,
        string userId2,
        CancellationToken token = default
    ) =>
        _dbContext
            .FriendRequests.Where(x =>
                x.RequesterUserId == userId1 && x.RecipientUserId == userId2
                || x.RequesterUserId == userId2 && x.RecipientUserId == userId1
            )
            .FirstOrDefaultAsync(token);

    public void DeleteFriendRequest(FriendRequest request) =>
        _dbContext.FriendRequests.Remove(request);

    public async Task AddFriendRequestAsync(
        FriendRequest request,
        CancellationToken token = default
    ) => await _dbContext.FriendRequests.AddAsync(request, token);

    public async Task AddFriendAsync(Friend friend, CancellationToken token = default) =>
        await _dbContext.Friends.AddAsync(friend, token);

    public Task<Friend?> GetFriendBetweenAsync(
        string userId1,
        string userId2,
        CancellationToken token = default
    ) =>
        _dbContext
            .Friends.Where(x =>
                (x.UserId1 == userId1 || x.UserId2 == userId1)
                && (x.UserId1 == userId2 || x.UserId2 == userId2)
            )
            .FirstOrDefaultAsync(token);
}
