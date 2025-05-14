using Chess2.Api.Models;
using Chess2.Api.Models.Entities;
using Chess2.Api.Services.Auth;
using Chess2.Api.TestInfrastructure.Fakes;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace Chess2.Api.TestInfrastructure.Utils;

public class AuthTestUtils(IAuthService authService, JwtSettings jwtSettings, DbContext dbContext)
{
    private readonly IAuthService _authService = authService;
    private readonly JwtSettings _jwtSettings = jwtSettings;
    private readonly DbContext _dbContext = dbContext;

    /// <summary>
    /// Calls <see cref="IsAuthenticated(ApiClient)"/> and asserts it is successful
    /// </summary>
    public async Task AssertAuthenticated(ApiClient apiClient) =>
        (await IsAuthenticated(apiClient)).Should().BeTrue();

    /// <summary>
    /// Calls <see cref="IsAuthenticated(ApiClient)"/> and asserts it is not successful
    /// </summary>
    public async Task AssertUnauthenticated(ApiClient apiClient) =>
        (await IsAuthenticated(apiClient)).Should().BeFalse();

    /// <summary>
    /// Attempt to call a test auth route to check if we are authenticated
    /// </summary>
    public async Task<bool> IsAuthenticated(ApiClient apiClient)
    {
        var testAuthResponse = await apiClient.Api.TestAuthAsync();
        return testAuthResponse.StatusCode == HttpStatusCode.NoContent;
    }

    /// <summary>
    /// Calls <see cref="IsGuestAuthenticated(ApiClient)"/> and asserts it is successful
    /// </summary>
    public async Task AssertGuestAuthenticated(ApiClient apiClient) =>
        (await IsGuestAuthenticated(apiClient)).Should().BeTrue();

    /// <summary>
    /// Calls <see cref="IsGuestAuthenticated(ApiClient)"/> and asserts it is not successful
    /// </summary>
    public async Task AssertGuestUnauthenticated(ApiClient apiClient) =>
        (await IsGuestAuthenticated(apiClient)).Should().BeFalse();

    /// <summary>
    /// Attempt to call the test guest auth route to check if we are guest authenticated
    /// </summary>
    public async Task<bool> IsGuestAuthenticated(ApiClient apiClient)
    {
        var testGuestAuthResponse = await apiClient.Api.TestGuestAsync();
        return testGuestAuthResponse.StatusCode == HttpStatusCode.NoContent;
    }

    public void AuthenticateWithUser(
        ApiClient apiClient,
        AuthedUser user,
        bool setAccessToken = true,
        bool setRefreshToken = true
    )
    {
        var accessToken = setAccessToken ? _authService.GenerateAuthTokensAsync(user);
        var refreshToken = setRefreshToken ? _authService.GenerateRefreshToken(user) : null;

        AuthenticateWithTokens(apiClient, accessToken, refreshToken);
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
                    Name = _jwtSettings.AccessTokenCookieName,
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
                    Name = _jwtSettings.RefreshTokenCookieName,
                    Value = refreshToken,
                    Path = "/api/auth/refresh",
                    Domain = apiClient.Client.BaseAddress?.Host,
                }
            );
        }
    }

    public async Task<AuthedUser> AuthenticateAsync(
        ApiClient apiClient,
        bool setAccessToken = true,
        bool setRefreshToken = true
    )
    {
        var user = await FakerUtils.StoreFakerAsync(_dbContext, new AuthedUserFaker());
        AuthenticateWithUser(apiClient, user, setAccessToken, setRefreshToken);
        return user;
    }
}
