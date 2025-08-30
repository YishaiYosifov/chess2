﻿using System.Net;
using Chess2.Api.Auth.Services;
using Chess2.Api.Shared.Models;
using Chess2.Api.TestInfrastructure.Fakes;
using Chess2.Api.Profile.Entities;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Chess2.Api.TestInfrastructure.Utils;

public record TestAuthResult(AuthedUser User, string? AccessToken, string? RefreshToken);

public record TestGuestResult(string UserId, string AccessToken);

public class AuthTestUtils(
    ITokenProvider tokenProvider,
    IRefreshTokenService refreshTokenService,
    JwtSettings jwtSettings,
    DbContext dbContext
)
{
    private readonly ITokenProvider _tokenProvider = tokenProvider;
    private readonly IRefreshTokenService _refreshTokenService = refreshTokenService;
    private readonly JwtSettings _jwtSettings = jwtSettings;
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
        var accessToken = setAccessToken ? _tokenProvider.GenerateAccessToken(user) : null;

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

    public TestGuestResult AuthenticateGuest(ApiClient apiClient, string guestId)
    {
        var accessToken = _tokenProvider.GenerateGuestToken(guestId);
        AuthenticateWithTokens(apiClient, accessToken);

        return new(guestId, accessToken);
    }
}
