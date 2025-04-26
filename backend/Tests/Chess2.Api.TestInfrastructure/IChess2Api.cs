using Chess2.Api.Models.DTOs;
using Refit;

namespace Chess2.Api.TestInfrastructure;

public interface IChess2Api
{
    #region Auth Controller
    [Post("/api/auth/signup")]
    Task<IApiResponse<PrivateUserOut>> SignupAsync([Body] SignupRequest userIn);

    [Post("/api/auth/login")]
    Task<IApiResponse<Tokens>> LoginAsync([Body] SigninRequest userLogin);

    [Post("/api/auth/refresh")]
    Task<IApiResponse> RefreshTokenAsync();

    [Post("/api/auth/guest")]
    Task<IApiResponse> CreateGuestAsync();

    [Post("/api/auth/test-authed")]
    Task<IApiResponse> TestAuthAsync();

    [Post("/api/auth/test-guest")]
    Task<IApiResponse> TestGuestAsync();
    #endregion

    #region Profile Controller
    [Get("/api/profile/me")]
    Task<IApiResponse<PrivateUserOut>> GetAuthedUserAsync();

    [Get("/api/profile/by-username/{username}")]
    Task<IApiResponse<UserOut>> GetUserAsync([AliasAs("username")] string username);

    [Patch("/api/profile/edit-profile")]
    Task<IApiResponse<PrivateUserOut>> EditProfileAsync(ProfileEdit profileUpdate);

    [Put("/api/profile/edit-username")]
    Task<IApiResponse<PrivateUserOut>> EditUsernameAsync([Body] string newUsername);
    #endregion
}
