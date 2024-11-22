using Chess2.Api.Models.DTOs;
using Refit;

namespace Chess2.Api.TestInfrastructure;

public interface IChess2Api
{
    #region Auth Controller
    [Post("/api/auth/signup")]
    Task<IApiResponse<PrivateUserOut>> SignupAsync([Body] UserIn userIn);

    [Post("/api/auth/login")]
    Task<IApiResponse<Tokens>> LoginAsync([Body] UserLogin userLogin);

    [Post("/api/auth/refresh")]
    Task<IApiResponse> RefreshTokenAsync();

    [Post("/api/auth/test")]
    Task<IApiResponse> TestAuthAsync();
    #endregion

    #region Profile Controller
    [Get("/api/profile/authed")]
    Task<IApiResponse<PrivateUserOut>> GetAuthedUserAsync();

    [Get("/api/profile/{username}")]
    Task<IApiResponse<UserOut>> GetUserAsync([AliasAs("username")] string username);

    [Patch("/api/profile/edit-profile")]
    Task<IApiResponse<PrivateUserOut>> EditProfileAsync(ProfileEdit profileUpdate);
    #endregion
}
