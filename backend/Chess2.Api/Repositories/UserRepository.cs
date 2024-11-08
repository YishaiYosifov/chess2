using Chess2.Api.Errors;
using Chess2.Api.Models;
using Chess2.Api.Models.Entities;
using Chess2.Api.Services;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace Chess2.Api.Repositories;

public interface IUserRepository
{
    public Task<bool> IsUsernameTaken(string username, CancellationToken cancellation = default);
    public Task<bool> IsEmailTaken(string email, CancellationToken cancellation = default);
    public Task AddUser(UserEntity user, CancellationToken cancellation = default);
}

public class UserRepository(Chess2DbContext dbContext, PasswordHasher passwordHasher) : IUserRepository
{
    private readonly Chess2DbContext _dbContext = dbContext;
    private readonly PasswordHasher _passwordHasher = passwordHasher;

    public async Task<bool> IsUsernameTaken(string username, CancellationToken cancellation = default) =>
        await _dbContext.Users.AnyAsync(user => user.Username == username, cancellation);

    public async Task<bool> IsEmailTaken(string email, CancellationToken cancellation = default) =>
        await _dbContext.Users.AnyAsync(user => user.Email == email, cancellation);

    public async Task AddUser(UserEntity user, CancellationToken cancellation = default)
    {
        await _dbContext.Users.AddAsync(user, cancellation);
        await _dbContext.SaveChangesAsync(cancellation);
    }
}
