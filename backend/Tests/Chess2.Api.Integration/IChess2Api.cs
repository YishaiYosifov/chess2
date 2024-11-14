using Chess2.Api.Models.DTOs;
using Refit;

namespace Chess2.Api.Integration;

public interface IChess2Api
{
    [Post("/api/auth/register")]
    Task<IApiResponse<PrivateUserOut>> RegisterAsync([Body] UserIn userIn);

    [Post("/api/auth/login")]
    Task<IApiResponse<Tokens>> LoginAsync([Body] UserLogin userLogin);

    [Post("/api/auth/refresh")]
    Task<IApiResponse> RefreshTokenAsync();

    [Post("/api/auth/test")]
    Task<IApiResponse> TestAuthAsync();

    [Get("/api/user/authed")]
    Task<IApiResponse<PrivateUserOut>> GetAuthedUser();
}
