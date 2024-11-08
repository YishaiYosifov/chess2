using Chess2.Api.Models.DTOs;
using Chess2.Api.Models.Entities;
using Chess2.Api.Repositories;
using FluentResults;
using FluentValidation;

namespace Chess2.Api.Services;

public interface IUserService
{
    Task<Result> RegisterUser(UserIn user, CancellationToken cancellation);
}

public class UserService(IValidator<UserIn> userValidator, IUserRepository userRepository, IPasswordHasher passwordHasher) : IUserService
{
    private readonly IValidator<UserIn> _userValidator = userValidator;
    private readonly IUserRepository _userRepository = userRepository;
    private readonly IPasswordHasher _passwordHasher = passwordHasher;

    public async Task<Result> RegisterUser(UserIn user, CancellationToken cancellation)
    {
        // TODO
        _userValidator.ValidateAndThrow(user);
        if (await _userRepository.IsUsernameTaken(user.Username, cancellation))
            return Result.Fail("TODO");
        if (await _userRepository.IsEmailTaken(user.Email, cancellation))
            return Result.Fail("TODO");

        var salt = _passwordHasher.GenerateSalt();
        var hash = await _passwordHasher.HashPassword(user.Password, salt);

        var userEntity = new UserEntity()
        {
            Username = user.Username,
            Email = user.Email,
            PasswordHash = hash,
            PasswordSalt = salt,
        };
        await _userRepository.AddUser(userEntity, cancellation);
        return Result.Ok();
    }
}
