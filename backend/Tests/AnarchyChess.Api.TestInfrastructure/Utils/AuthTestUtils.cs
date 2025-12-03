using System.Net;
using AnarchyChess.Api.Auth.Services;
using AnarchyChess.Api.Profile.Entities;
using AnarchyChess.Api.Profile.Models;
using AnarchyChess.Api.Shared.Models;
using AnarchyChess.Api.TestInfrastructure.Fakes;
using AwesomeAssertions;
using Microsoft.EntityFrameworkCore;

namespace AnarchyChess.Api.TestInfrastructure.Utils;

public record TestAuthResult(AuthedUser User, string? AccessToken, string? RefreshToken);

public record TestGuestResult(UserId UserId, string AccessToken);

public class AuthTestUtils(
    ITokenProvider tokenProvider,
    IRefreshTokenService refreshTokenService,
    AuthSettings authSettings,
    DbContext dbContext
)
{
    private readonly ITokenProvider _tokenProvider = tokenProvider;
    private readonly IRefreshTokenService _refreshTokenService = refreshTokenService;
    private readonly AuthSettings _authSettings = authSettings;
    private readonly DbContext _dbContext = dbContext;

    /// <summary>
    /// Calls <see cref="IsAuthenticated(ApiClient)"/> and asserts it is successful
    /// </summary>
    public static async Task AssertAuthenticated(ApiClient apiClient) =>
        (await IsAuthenticated(apiClient)).Should().BeTrue();

    /// <summary>
    /// Calls <see cref="IsAuthenticated(ApiClient)"/> and asserts it is not successful
    /// </summary>
    public static async Task AssertUnauthenticated(ApiClient apiClient) =>
        (await IsAuthenticated(apiClient)).Should().BeFalse();

    /// <summary>
    /// Attempt to call a test auth route to check if we are authenticated
    /// </summary>
    public static async Task<bool> IsAuthenticated(ApiClient apiClient)
    {
        var testAuthResponse = await apiClient.Api.TestAuthAsync();
        return testAuthResponse.StatusCode == HttpStatusCode.NoContent;
    }

    /// <summary>
    /// Calls <see cref="IsGuestAuthenticated(ApiClient)"/> and asserts it is successful
    /// </summary>
    public static async Task AssertGuestAuthenticated(ApiClient apiClient) =>
        (await IsGuestAuthenticated(apiClient)).Should().BeTrue();

    /// <summary>
    /// Calls <see cref="IsGuestAuthenticated(ApiClient)"/> and asserts it is not successful
    /// </summary>
    public static async Task AssertGuestUnauthenticated(ApiClient apiClient) =>
        (await IsGuestAuthenticated(apiClient)).Should().BeFalse();

    /// <summary>
    /// Attempt to call the test guest auth route to check if we are guest authenticated
    /// </summary>
    public static async Task<bool> IsGuestAuthenticated(ApiClient apiClient)
    {
        var testGuestAuthResponse = await apiClient.Api.TestGuestAsync();
        return testGuestAuthResponse.StatusCode == HttpStatusCode.NoContent;
    }

    public async Task<TestAuthResult> AuthenticateWithUserAsync(
        ApiClient apiClient,
        AuthedUser user,
        bool setAccessToken = true,
        bool setRefreshToken = true
    )
    {
        var accessToken = setAccessToken ? _tokenProvider.GenerateAccessToken(user).Value : null;

        string? refreshToken = null;
        if (setRefreshToken)
        {
            var refreshTokenRecord = await _refreshTokenService.CreateRefreshTokenAsync(user);
            refreshToken = _tokenProvider.GenerateRefreshToken(user, refreshTokenRecord.Jti);
            await _dbContext.SaveChangesAsync();
        }

        AuthenticateWithTokens(apiClient, accessToken, refreshToken);
        return new(user, accessToken, refreshToken);
    }

    public void AuthenticateWithTokens(
        ApiClient apiClient,
        string? accessToken = null,
        string? refreshToken = null
    )
    {
        if (accessToken is not null)
        {
            apiClient.CookieContainer.Add(
                new Cookie()
                {
                    Name = _authSettings.AccessTokenCookieName,
                    Value = accessToken,
                    Domain = apiClient.Client.BaseAddress?.Host,
                }
            );
        }

        if (refreshToken is not null)
        {
            apiClient.CookieContainer.Add(
                new Cookie()
                {
                    Name = _authSettings.RefreshTokenCookieName,
                    Value = refreshToken,
                    Path = "/api/auth/refresh",
                    Domain = apiClient.Client.BaseAddress?.Host,
                }
            );
        }
    }

    public async Task<TestAuthResult> AuthenticateAsync(
        ApiClient apiClient,
        bool setAccessToken = true,
        bool setRefreshToken = true
    )
    {
        var user = new AuthedUserFaker().Generate();
        await _dbContext.AddAsync(user);
        await _dbContext.SaveChangesAsync();

        var authResult = await AuthenticateWithUserAsync(
            apiClient,
            user,
            setAccessToken,
            setRefreshToken
        );

        return authResult;
    }

    public TestGuestResult AuthenticateGuest(ApiClient apiClient, UserId? guestId = null)
    {
        var userId = guestId ?? UserId.Guest();
        var accessToken = _tokenProvider.GenerateGuestToken(userId);
        AuthenticateWithTokens(apiClient, accessToken);

        return new(userId, accessToken);
    }
}
