using Chess2.Api.Errors;
using Chess2.Api.Models.Entities;
using Chess2.Api.Repositories;
using ErrorOr;

namespace Chess2.Api.Services;

public interface IUserService
{
    Task<ErrorOr<User>> GetUserByUsernameAsync(string username);
}

public class UserService(IUserRepository userRepository) : IUserService
{
    private readonly IUserRepository _userRepository = userRepository;

    /// <summary>
    /// Gets the user by its username.
    /// If it was not found, return a not found error
    /// </summary>
    public async Task<ErrorOr<User>> GetUserByUsernameAsync(string username)
    {
        var user = await _userRepository.GetByUsernameAsync(username);
        return user is null ? UserErrors.UserNotFound : user;
    }
}
