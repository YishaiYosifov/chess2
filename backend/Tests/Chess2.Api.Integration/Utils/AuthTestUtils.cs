using Chess2.Api.Integration.Fakes;
using Chess2.Api.Models;
using Chess2.Api.Models.DTOs;
using Chess2.Api.Models.Entities;
using FluentAssertions;
using Refit;
using System.Net;

namespace Chess2.Api.Integration.Utils;

public static class AuthTestUtils
{
    /// <summary>
    /// Calls <see cref="IsAuthenticated(IChess2Api)"/> and asserts it is successful
    /// </summary>
    public static async Task AssertAuthenticated(IChess2Api apiClient)
    {
        (await IsAuthenticated(apiClient)).Should().BeTrue();
    }

    /// <summary>
    /// Calls <see cref="IsAuthenticated(IChess2Api)"/> and asserts it is not successful
    /// </summary>
    public static async Task AssertUnauthenticated(IChess2Api apiClient)
    {
        (await IsAuthenticated(apiClient)).Should().BeFalse();
    }

    /// <summary>
    /// Attempt to call a test route to check if we are authenticated
    /// </summary>
    public static async Task<bool> IsAuthenticated(IChess2Api apiClient)
    {
        var testAuthResponse = await apiClient.TestAuthAsync();
        return testAuthResponse.StatusCode == HttpStatusCode.NoContent;
    }

    public static Task Authenticate(IChess2Api apiClient, User user, string? password = null) =>
        apiClient.LoginAsync(new()
        {
            UsernameOrEmail = user.Username,
            Password = password ?? UserFaker.Password
        });

    public async static Task<User> Authenticate(IChess2Api apiClient, Chess2DbContext dbContext)
    {
        var user = await FakerUtils.StoreFaker(dbContext, new UserFaker());
        await Authenticate(apiClient, user, UserFaker.Password);
        return user;
    }
}
