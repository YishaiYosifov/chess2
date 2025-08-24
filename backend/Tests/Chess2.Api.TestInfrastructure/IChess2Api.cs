using Chess2.Api.ArchivedGames.Models;
using Chess2.Api.Auth.DTOs;
using Chess2.Api.GameSnapshot.Models;
using Chess2.Api.Shared.Models;
using Chess2.Api.Users.DTOs;
using Microsoft.AspNetCore.JsonPatch;
using Refit;

namespace Chess2.Api.TestInfrastructure;

public interface IChess2Api
{
    #region Auth Controller

    [Get("/api/oauth/signin/{provider}")]
    Task<IApiResponse<Tokens>> OAuthLoginAsync([AliasAs("provider")] string provider);

    [Post("/api/auth/refresh")]
    Task<IApiResponse> RefreshTokenAsync();

    [Post("/api/auth/guest")]
    Task<IApiResponse> CreateGuestAsync();

    [Post("/api/auth/test-auth")]
    Task<IApiResponse> TestAuthAsync();

    [Post("/api/auth/test-guest-auth")]
    Task<IApiResponse> TestGuestAsync();
    #endregion

    #region Profile Controller
    [Get("/api/profile/me")]
    Task<IApiResponse<PublicUser>> GetSessionUserAuthedAsync();

    [Get("/api/profile/me")]
    Task<IApiResponse<GuestUser>> GetSessionUserGuestAsync();

    [Get("/api/profile/by-username/{username}")]
    Task<IApiResponse<PublicUser>> GetUserAsync([AliasAs("username")] string username);

    [Patch("/api/profile/edit-profile")]
    Task<IApiResponse> EditProfileAsync([Body] JsonPatchDocument<ProfileEditRequest> profileEdit);

    [Put("/api/profile/edit-username")]
    [Headers("Content-Type: application/json; charset=utf-8")]
    Task<IApiResponse> EditUsernameAsync([Body] string newUsername);
    #endregion

    #region Game Controller
    [Get("/api/game/{gameToken}")]
    Task<IApiResponse<GameState>> GetGameAsync([AliasAs("gameToken")] string gameToken);

    [Get("/api/game/results/{userId}")]
    Task<IApiResponse<PagedResult<GameSummaryDto>>> GetGameResultsAsync(
        [AliasAs("userId")] string userId,
        [Query] PaginationQuery pagination
    );
    #endregion
}
