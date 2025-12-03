using AnarchyChess.Api.ArchivedGames.Models;
using AnarchyChess.Api.Challenges.Models;
using AnarchyChess.Api.GameSnapshot.Models;
using AnarchyChess.Api.Matchmaking.Models;
using AnarchyChess.Api.Pagination.Models;
using AnarchyChess.Api.Preferences.DTOs;
using AnarchyChess.Api.Profile.DTOs;
using AnarchyChess.Api.Quests.DTOs;
using AnarchyChess.Api.Shared.Models;
using AnarchyChess.Api.Streaks.Models;
using AnarchyChess.Api.UserRating.Models;
using Refit;

namespace AnarchyChess.Api.TestInfrastructure;

public interface IAnarchyChessApi
{
    #region Auth

    [Get("/api/oauth/signin/{provider}")]
    Task<IApiResponse> SignInOAuthAsync(string provider);

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
    Task<IApiResponse<PublicUser>> GetUserByUsernameAsync(string username);

    [Get("/api/profile/by-id/{userId}")]
    Task<IApiResponse<PublicUser>> GetUserByIdAsync(string userId);

    [Put("/api/profile/edit-profile")]
    Task<IApiResponse> EditProfileAsync(ProfileEditRequest profileEdit);

    [Put("/api/profile/edit-username")]
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

    #region Ratings
    [Get("/api/rating/{userId}/archive")]
    Task<IApiResponse<IEnumerable<RatingOverview>>> GetRatingArchivesAsync(
        string userId,
        [Query] DateTime? since
    );

    [Get("/api/rating/{userId}")]
    Task<IApiResponse<IEnumerable<CurrentRatingStatus>>> GetCurrentRatingsAsync(string userId);
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

    [Get("/api/social/blocked")]
    Task<IApiResponse<PagedResult<MinimalProfile>>> GetBlockedUsersAsync(
        [Query] PaginationQuery pagination
    );

    [Get("/api/social/block/{userId}/exists")]
    Task<IApiResponse<bool>> GetHasBlockedAsync(string userId);

    [Post("/api/social/block/{userId}")]
    Task<IApiResponse> BlockUserAsync(string userId);

    [Delete("/api/social/block/{userId}")]
    Task<IApiResponse> UnblockUserAsync(string userId);
    #endregion

    #region Preferences
    [Get("/api/preference")]
    Task<IApiResponse<PreferenceDto>> GetPreferencesAsync();

    [Put("/api/preference")]
    Task<IApiResponse> SetPreferencesAsync(PreferenceDto preferences);
    #endregion

    #region Quests
    [Get("/api/quests")]
    Task<IApiResponse<QuestDto>> GetDailyQuestAsync();

    [Post("/api/quests/replace")]
    Task<IApiResponse<QuestDto>> ReplaceDailyQuestAsync();

    [Post("/api/quests/claim")]
    Task<IApiResponse<QuestDto>> CollectQuestRewardAsync();

    [Get("/api/quests/points/{userId}")]
    Task<IApiResponse<int>> GetUserQuestPointsAsync(string userId);

    [Get("/api/quests/leaderboard")]
    Task<IApiResponse<PagedResult<QuestPointsDto>>> GetQuestLeaderboardAsync(
        [Query] PaginationQuery pagination
    );

    [Get("/api/quests/leaderboard/me")]
    Task<IApiResponse<int>> GetMyQuestRankingAsync();
    #endregion

    #region Challenges
    [Put("/api/challenge")]
    Task<IApiResponse<ChallengeRequest>> CreateChallengeAsync(
        [Query] string? recipientId,
        PoolKeyRequest pool
    );

    [Get("/api/challenge/by-id/{challengeToken}")]
    Task<IApiResponse<ChallengeRequest>> GetChallengeAsync(string challengeToken);

    [Delete("/api/challenge/by-id/{challengeToken}")]
    Task<IApiResponse> CancelChallengeAsync(string challengeToken);

    [Post("/api/challenge/by-id/{challengeToken}/accept")]
    Task<IApiResponse<string>> AcceptChallengeAsync(string challengeToken);

    [Delete("/api/challenge/incoming")]
    Task<IApiResponse> CancelAllIncomingChallengesAsync();
    #endregion

    #region Win Streaks
    [Get("/api/winStreak/leaderboard")]
    Task<IApiResponse<PagedResult<WinStreakDto>>> GetWinStreakLeaderboardAsync(
        [Query] PaginationQuery pagination
    );

    [Get("/api/winStreak/me")]
    Task<IApiResponse<MyWinStreakStats>> GetMyWinStreakStatsAsync();
    #endregion
}
