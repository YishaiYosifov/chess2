using Chess2.Api.Models.DTOs;
using Refit;

namespace Chess2.Api.Integration;

public interface IChess2Api
{
    [Post("/api/auth/register")]
    Task<IApiResponse<UserOut>> RegisterAsync([Body] UserIn userIn);

    [Post("/api/auth/login")]
    Task<IApiResponse<UserOut>> LoginAsync([Body] UserLogin userLogin);

    [Post("/api/auth/test")]
    Task<IApiResponse> TestAuthAsync();
}
