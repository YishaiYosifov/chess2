using Chess2.Api.Errors;
using Chess2.Api.Models;
using Chess2.Api.Models.Requests;
using Chess2.Api.Services;
using Microsoft.EntityFrameworkCore;

namespace Chess2.Api.Repositories;

public interface IUserRepository
{
    public Task<bool> IsUsernameTaken(string username, CancellationToken cancellation = default);
    public Task<bool> IsEmailTaken(string email, CancellationToken cancellation = default);
    public Task<Result<User>> RegisterUser(UserIn user, CancellationToken cancellation = default);
}

public class UserRepository(Chess2DbContext dbContext) : IUserRepository
{
    private readonly Chess2DbContext _dbContext = dbContext;

    public async Task<bool> IsUsernameTaken(string username, CancellationToken cancellation = default) =>
        await _dbContext.Users.AnyAsync(user => user.Username.ToLower() == username.ToLower(), cancellation);

    public async Task<bool> IsEmailTaken(string email, CancellationToken cancellation = default) =>
        await _dbContext.Users.AnyAsync(user => user.Email.ToLower() == email.ToLower(), cancellation);

    public async Task<Result<User>> RegisterUser(UserIn user, CancellationToken cancellation = default)
    {
        if (await IsUsernameTaken(user.Username, cancellation))
            return Result<User>.Failure(UserErrors.UsernameTaken);
        if (await IsEmailTaken(user.Email, cancellation))
            return Result<User>.Failure(UserErrors.EmailTaken);

        var dbUser = new User()
        {
            Username = user.Username,
            Email = user.Email
        };
        await _dbContext.Users.AddAsync(dbUser, cancellation);
        await _dbContext.SaveChangesAsync(cancellation);
        return Result<User>.Success(dbUser);
    }
}
