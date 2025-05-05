using Chess2.Api.Models.DTOs;
using Microsoft.AspNetCore.JsonPatch;
using Refit;

namespace Chess2.Api.TestInfrastructure;

public interface IChess2Api
{
    #region Auth Controller
    [Post("/api/auth/signup")]
    Task<IApiResponse<PrivateUserOut>> SignupAsync([Body] SignupRequest userIn);

    [Post("/api/auth/signin")]
    Task<IApiResponse<AuthResponseDTO>> SigninAsync([Body] SigninRequest userLogin);

    [Post("/api/auth/refresh")]
    Task<IApiResponse<AuthResponseDTO>> RefreshTokenAsync();

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
    Task<IApiResponse> EditProfileAsync([Body] JsonPatchDocument<ProfileEditRequest> profileEdit);

    [Put("/api/profile/edit-username")]
    [Headers("Content-Type: application/json; charset=utf-8")]
    Task<IApiResponse<PrivateUserOut>> EditUsernameAsync([Body] string newUsername);
    #endregion
}
