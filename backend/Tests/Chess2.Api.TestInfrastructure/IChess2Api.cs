using Chess2.Api.Models.DTOs;
using Refit;

namespace Chess2.Api.TestInfrastructure;

public interface IChess2Api
{
    #region Auth Controller
    [Post("/api/auth/register")]
    Task<IApiResponse<PrivateUserOut>> RegisterAsync([Body] UserIn userIn);

    [Post("/api/auth/login")]
    Task<IApiResponse<Tokens>> LoginAsync([Body] UserLogin userLogin);

    [Post("/api/auth/refresh")]
    Task<IApiResponse> RefreshTokenAsync();

    [Post("/api/auth/test")]
    Task<IApiResponse> TestAuthAsync();
    #endregion

    #region User Controller
    [Get("/api/user/authed")]
    Task<IApiResponse<PrivateUserOut>> GetAuthedUserAsync();

    [Get("/api/user/{username}")]
    Task<IApiResponse<UserOut>> GetUserAsync([AliasAs("username")] string username);

    [Patch("/api/user/update-profile")]
    Task<IApiResponse<PrivateUserOut>> UpdateUserProfileAsync(UserProfileUpdate profileUpdate);
    #endregion
}
