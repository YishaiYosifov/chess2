using AnarchyChess.Api.Donations.Entities;
using AnarchyChess.Api.Infrastructure;

namespace AnarchyChess.Api.Donations.Repositories;

public interface IDonationRepository
{
    Task AddAsync(Donation donation, CancellationToken token = default);
    Task<Donation?> GetByEmailAsync(string email, CancellationToken token = default);
}

public class DonationRepository(ApplicationDbContext dbContext) : IDonationRepository
{
    private readonly ApplicationDbContext _dbContext = dbContext;

    public async Task<Donation?> GetByEmailAsync(string email, CancellationToken token = default) =>
        await _dbContext.Donations.FindAsync([email], cancellationToken: token);

    public async Task AddAsync(Donation donation, CancellationToken token = default) =>
        await _dbContext.Donations.AddAsync(donation, token);
}
