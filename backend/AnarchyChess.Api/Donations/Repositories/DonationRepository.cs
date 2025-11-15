using AnarchyChess.Api.Donations.Entities;
using AnarchyChess.Api.Infrastructure;
using AnarchyChess.Api.Pagination.Extensions;
using AnarchyChess.Api.Pagination.Models;
using Microsoft.EntityFrameworkCore;

namespace AnarchyChess.Api.Donations.Repositories;

public interface IDonationRepository
{
    Task AddAsync(Donation donation, CancellationToken token = default);
    Task<Donation?> GetByEmailAsync(string email, CancellationToken token = default);
    Task<List<Donation>> GetPaginatedLeaderboardAsync(
        PaginationQuery pagination,
        CancellationToken token = default
    );
    Task<int> GetTotalCountAsync(CancellationToken token = default);
}

public class DonationRepository(ApplicationDbContext dbContext) : IDonationRepository
{
    private readonly ApplicationDbContext _dbContext = dbContext;

    public Task<List<Donation>> GetPaginatedLeaderboardAsync(
        PaginationQuery pagination,
        CancellationToken token = default
    ) =>
        _dbContext
            .Donations.OrderByDescending(x => x.TotalAmount)
            .Paginate(pagination)
            .ToListAsync(token);

    public Task<int> GetTotalCountAsync(CancellationToken token = default) =>
        _dbContext.Donations.CountAsync(token);

    public async Task<Donation?> GetByEmailAsync(string email, CancellationToken token = default) =>
        await _dbContext.Donations.FindAsync([email], cancellationToken: token);

    public async Task AddAsync(Donation donation, CancellationToken token = default) =>
        await _dbContext.Donations.AddAsync(donation, token);
}
