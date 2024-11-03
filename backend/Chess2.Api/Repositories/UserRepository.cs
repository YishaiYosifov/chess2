using Chess2.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Chess2.Api.Repositories;

public interface IUserRepository
{
    public Task<bool> IsUsernameTaken(string username, CancellationToken cancellation = default);
    public Task<bool> IsEmailTaken(string email, CancellationToken cancellation = default);
}

public class UserRepository(Chess2DbContext dbContext) : IUserRepository
{
    private readonly Chess2DbContext _dbContext = dbContext;

    public async Task<bool> IsUsernameTaken(string username, CancellationToken cancellation = default) =>
        await _dbContext.Users.AnyAsync(user => user.Username == username, cancellation);

    public async Task<bool> IsEmailTaken(string email, CancellationToken cancellation = default) =>
        await _dbContext.Users.AnyAsync(user => user.Email == email, cancellation);
}
