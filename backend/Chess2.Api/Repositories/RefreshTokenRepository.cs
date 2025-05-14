using Chess2.Api.Models;
using Chess2.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Chess2.Api.Repositories;

public class RefreshTokenRepository(ApplicationDbContext dbContext)
{
    private readonly ApplicationDbContext _dbContext = dbContext;

    public Task<RefreshToken?> GetTokenAsync(string token) =>
        _dbContext.RefreshTokens.SingleOrDefaultAsync(t => t.Token == token);
}
