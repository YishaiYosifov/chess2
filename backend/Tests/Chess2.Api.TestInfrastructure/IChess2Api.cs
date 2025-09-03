using Chess2.Api.ArchivedGames.Models;
using Chess2.Api.Auth.DTOs;
using Chess2.Api.GameSnapshot.Models;
using Chess2.Api.Pagination.Models;
using Chess2.Api.Profile.DTOs;
using Chess2.Api.Shared.Models;
using Refit;

namespace Chess2.Api.TestInfrastructure;

public interface IChess2Api
{
    #region Auth

    [Get("/api/oauth/signin/{provider}")]
    Task<IApiResponse<Tokens>> OAuthLoginAsync(string provider);

    [Post("/api/auth/refresh")]
    Task<IApiResponse> RefreshTokenAsync();

    [Post("/api/auth/guest")]
    Task<IApiResponse> CreateGuestAsync();

    [Post("/api/auth/test-auth")]
    Task<IApiResponse> TestAuthAsync();

    [Post("/api/auth/test-guest-auth")]
    Task<IApiResponse> TestGuestAsync();
    #endregion

    #region Profile
    [Get("/api/profile/me")]
    Task<IApiResponse<PrivateUser>> GetSessionUserAuthedAsync();

    [Get("/api/profile/me")]
    Task<IApiResponse<GuestUser>> GetSessionUserGuestAsync();

    [Get("/api/profile/by-username/{username}")]
    Task<IApiResponse<PublicUser>> GetUserAsync(string username);

    [Put("/api/profile/edit-profile")]
    Task<IApiResponse> EditProfileAsync(ProfileEditRequest profileEdit);

    [Put("/api/profile/edit-username")]
    [Headers("Content-Type: application/json; charset=utf-8")]
    Task<IApiResponse> EditUsernameAsync(UsernameEditRequest usernameEdit);

    [Multipart]
    [Put("/api/profile/profile-picture")]
    Task<IApiResponse> UploadProfilePictureAsync(StreamPart file);

    [Delete("/api/profile/profile-picture")]
    Task<IApiResponse> DeleteProfilePictureAsync();

    [Get("/api/profile/profile-picture/{userId}")]
    Task<IApiResponse<Stream>> GetProfilePictureAsync(
        string userId,
        [Header("If-None-Match")] string? etag = null
    );
    #endregion

    #region Game
    [Get("/api/game/{gameToken}")]
    Task<IApiResponse<GameState>> GetGameAsync(string gameToken);

    [Get("/api/game/results/{userId}")]
    Task<IApiResponse<PagedResult<GameSummaryDto>>> GetGameResultsAsync(
        string userId,
        [Query] PaginationQuery pagination
    );
    #endregion

    #region Social
    [Get("/api/social/starred/{userId}")]
    Task<IApiResponse<PagedResult<MinimalProfile>>> GetStarredUsersAsync(
        string userId,
        [Query] PaginationQuery pagination
    );

    [Get("/api/social/stars/{userId}")]
    Task<IApiResponse<int>> GetStarsReceivedCountAsync(string userId);

    [Get("/api/social/star/{userId}/exists")]
    Task<IApiResponse<bool>> GetIsStarredAsync(string userId);

    [Post("/api/social/star/{userId}")]
    Task<IApiResponse> AddStarAsync(string userId);

    [Delete("/api/social/star/{userId}")]
    Task<IApiResponse> RemoveStarAsync(string userId);
    #endregion
}
