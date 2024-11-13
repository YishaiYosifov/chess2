using Chess2.Api.Models.DTOs;
using Chess2.Api.Models.Entities;
using Refit;

namespace Chess2.Api.Integration;

public interface IChess2Api
{
    [Post("/api/auth/register")]
    Task<IApiResponse<UserOut>> RegisterAsync([Body] UserIn userIn);

    [Post("/api/auth/login")]
    Task<IApiResponse<Tokens>> LoginAsync([Body] UserLogin userLogin);

    Task<IApiResponse<Tokens>> LoginAsync(User user, string password) =>
        LoginAsync(new UserLogin()
        {
            UsernameOrEmail = user.Username,
            Password = password,
        });

    [Post("/api/auth/refresh")]
    Task<IApiResponse> RefreshTokenAsync();

    [Post("/api/auth/test")]
    Task<IApiResponse> TestAuthAsync();
}
