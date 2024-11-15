using Chess2.Api.Models;
using Chess2.Api.Models.DTOs;
using Chess2.Api.Models.Entities;
using Chess2.Api.TestInfrastructure;
using FluentAssertions;
using System.Net;

namespace Chess2.Api.Functional.Utils;

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

    public static async Task<Tokens> Authenticate(IChess2Api apiClient, User user, string? password = null)
    {
        var response = await apiClient.LoginAsync(new()
        {
            UsernameOrEmail = user.Username,
            Password = password ?? UserFaker.Password
        });
        response.IsSuccessful.Should().BeTrue();

        return response.Content!;
    }


    public async static Task<(User User, Tokens Tokens)> Authenticate(IChess2Api apiClient, Chess2DbContext dbContext)
    {
        var user = await FakerUtils.StoreFaker(dbContext, new UserFaker());
        var tokens = await Authenticate(apiClient, user, UserFaker.Password);
        return (user, tokens);
    }
}
