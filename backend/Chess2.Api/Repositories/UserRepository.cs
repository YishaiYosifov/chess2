using Chess2.Api.Models;
using Chess2.Api.Models.DTOs;
using Chess2.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Chess2.Api.Repositories;

public interface IUserRepository
{
    public Task<AuthedUser?> GetByUsernameAsync(
        string username,
        CancellationToken cancellation = default
    );
    public Task<AuthedUser?> GetByEmailAsync(
        string email,
        CancellationToken cancellation = default
    );
    public Task<AuthedUser?> GetByUserIdAsync(int userId, CancellationToken cancellation = default);
    public Task AddUserAsync(AuthedUser user, CancellationToken cancellation = default);
    public Task<AuthedUser> EditProfileAsync(
        AuthedUser user,
        ProfileEdit userEdit,
        CancellationToken cancellation = default
    );
}

public class UserRepository(Chess2DbContext dbContext) : IUserRepository
{
    private readonly Chess2DbContext _dbContext = dbContext;

    public async Task<AuthedUser?> GetByUsernameAsync(
        string username,
        CancellationToken cancellation = default
    ) =>
        await _dbContext.Users.FirstOrDefaultAsync(user => user.Username == username, cancellation);

    public async Task<AuthedUser?> GetByEmailAsync(
        string email,
        CancellationToken cancellation = default
    ) => await _dbContext.Users.FirstOrDefaultAsync(user => user.Email == email, cancellation);

    public async Task<AuthedUser?> GetByUserIdAsync(
        int userId,
        CancellationToken cancellation = default
    ) =>
        await _dbContext.Users.FirstOrDefaultAsync(
            user => user.AuthedUserId == userId,
            cancellation
        );

    public async Task AddUserAsync(AuthedUser user, CancellationToken cancellation = default)
    {
        await _dbContext.Users.AddAsync(user, cancellation);
        await _dbContext.SaveChangesAsync(cancellation);
    }

    /// <summary>
    /// Update the profile data of a user.
    /// This method takes the properties of <see cref="ProfileEdit"/> and
    /// updates them in the user if it's not null
    /// </summary>
    public async Task<AuthedUser> EditProfileAsync(
        AuthedUser user,
        ProfileEdit userEdit,
        CancellationToken cancellation = default
    )
    {
        foreach (var prop in userEdit.GetType().GetProperties())
        {
            var value = prop.GetValue(userEdit);
            if (value is null)
                continue;

            var userProperty = user.GetType().GetProperty(prop.Name);
            if (userProperty is not null && userProperty.CanWrite)
                userProperty.SetValue(user, value);
        }
        await _dbContext.SaveChangesAsync(cancellation);

        return user;
    }
}
