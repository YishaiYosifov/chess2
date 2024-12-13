using System.Net;
using Chess2.Api.Models;
using Chess2.Api.Models.DTOs;
using Chess2.Api.Models.Entities;
using Chess2.Api.TestInfrastructure;
using Chess2.Api.TestInfrastructure.Fakes;
using Chess2.Api.TestInfrastructure.Utils;
using FluentAssertions;

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

    public static async Task<Tokens> Authenticate(
        IChess2Api apiClient,
        AuthedUser user,
        string? password = null
    )
    {
        var response = await apiClient.LoginAsync(
            new()
            {
                UsernameOrEmail = user.Username,
                Password = password ?? AuthedUserFaker.Password,
            }
        );
        response.IsSuccessful.Should().BeTrue();

        return response.Content!;
    }

    public static async Task<(AuthedUser User, Tokens Tokens)> Authenticate(
        IChess2Api apiClient,
        Chess2DbContext dbContext
    )
    {
        var user = await FakerUtils.StoreFaker(dbContext, new AuthedUserFaker());
        var tokens = await Authenticate(apiClient, user, AuthedUserFaker.Password);
        return (user, tokens);
    }
}
