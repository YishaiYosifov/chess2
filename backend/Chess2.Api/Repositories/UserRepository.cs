using Chess2.Api.Models;
using Chess2.Api.Models.DTOs;
using Chess2.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Chess2.Api.Repositories;

public interface IUserRepository
{
    public Task<User?> GetByUsernameAsync(string username, CancellationToken cancellation = default);
    public Task<User?> GetByEmailAsync(string email, CancellationToken cancellation = default);
    public Task<User?> GetByUserIdAsync(int userId, CancellationToken cancellation = default);
    public Task AddUserAsync(User user, CancellationToken cancellation = default);
    public Task<User> EditProfileAsync(User user, ProfileEdit userEdit, CancellationToken cancellation = default);
}

public class UserRepository(Chess2DbContext dbContext) : IUserRepository
{
    private readonly Chess2DbContext _dbContext = dbContext;

    public async Task<User?> GetByUsernameAsync(string username, CancellationToken cancellation = default) =>
        await _dbContext.Users.FirstOrDefaultAsync(user => user.Username == username, cancellation);

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellation = default) =>
        await _dbContext.Users.FirstOrDefaultAsync(user => user.Email == email, cancellation);

    public async Task<User?> GetByUserIdAsync(int userId, CancellationToken cancellation = default) =>
        await _dbContext.Users.FirstOrDefaultAsync(user => user.UserId == userId, cancellation);

    public async Task AddUserAsync(User user, CancellationToken cancellation = default)
    {
        await _dbContext.Users.AddAsync(user, cancellation);
        await _dbContext.SaveChangesAsync(cancellation);
    }

    /// <summary>
    /// Update the profile data of a user.
    /// This method takes the properties of <see cref="ProfileEdit"/> and
    /// updates them in the user if it's not null
    /// </summary>
    public async Task<User> EditProfileAsync(User user, ProfileEdit userEdit, CancellationToken cancellation = default)
    {
        foreach (var prop in userEdit.GetType().GetProperties())
        {
            var value = prop.GetValue(userEdit);
            if (value is null) continue;

            var userProperty = user.GetType().GetProperty(prop.Name);
            if (userProperty is not null && userProperty.CanWrite)
                userProperty.SetValue(user, value);
        }
        await _dbContext.SaveChangesAsync(cancellation);

        return user;
    }
}
