using Chess2.Api.Models;
using Chess2.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Chess2.Api.Repositories;

public interface IRefreshTokenRepository
{
    Task AddRefreshTokenAsync(RefreshToken refreshToken, CancellationToken token = default);
    Task<RefreshToken?> GetTokenByJtiAsync(string jti, CancellationToken token = default);
}

public class RefreshTokenRepository(ApplicationDbContext dbContext) : IRefreshTokenRepository
{
    private readonly ApplicationDbContext _dbContext = dbContext;

    public Task<RefreshToken?> GetTokenByJtiAsync(string jti, CancellationToken token = default) =>
        _dbContext.RefreshTokens.SingleOrDefaultAsync(t => t.Jti == jti, token);

    public async Task AddRefreshTokenAsync(
        RefreshToken refreshToken,
        CancellationToken token = default
    ) => await _dbContext.RefreshTokens.AddAsync(refreshToken, token);
}
