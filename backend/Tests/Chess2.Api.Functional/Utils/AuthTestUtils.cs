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
    public static async Task AssertAuthenticated(IChess2Api apiClient) =>
        (await IsAuthenticated(apiClient)).Should().BeTrue();

    /// <summary>
    /// Calls <see cref="IsAuthenticated(IChess2Api)"/> and asserts it is not successful
    /// </summary>
    public static async Task AssertUnauthenticated(IChess2Api apiClient) =>
        (await IsAuthenticated(apiClient)).Should().BeFalse();

    /// <summary>
    /// Attempt to call a test auth route to check if we are authenticated
    /// </summary>
    public static async Task<bool> IsAuthenticated(IChess2Api apiClient)
    {
        var testAuthResponse = await apiClient.TestAuthAsync();
        return testAuthResponse.StatusCode == HttpStatusCode.NoContent;
    }

    /// <summary>
    /// Calls <see cref="IsGuestAuthenticated(IChess2Api)"/> and asserts it is successful
    /// </summary>
    public static async Task AssertGuestAuthenticated(IChess2Api apiClient) =>
        (await IsGuestAuthenticated(apiClient)).Should().BeTrue();

    /// <summary>
    /// Calls <see cref="IsGuestAuthenticated(IChess2Api)"/> and asserts it is not successful
    /// </summary>
    public static async Task AssertGuestUnauthenticated(IChess2Api apiClient) =>
        (await IsGuestAuthenticated(apiClient)).Should().BeFalse();

    /// <summary>
    /// Attempt to call the test guest auth route to check if we are guest authenticated
    /// </summary>
    public static async Task<bool> IsGuestAuthenticated(IChess2Api apiClient)
    {
        var testGuestAuthResponse = await apiClient.TestGuestAsync();
        return testGuestAuthResponse.StatusCode == HttpStatusCode.NoContent;
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
                UsernameOrEmail = user.UserName!,
                Password = password ?? AuthedUserFaker.Password,
            }
        );
        response.IsSuccessful.Should().BeTrue();

        return response.Content!;
    }

    public static async Task<(AuthedUser User, Tokens Tokens)> Authenticate(
        IChess2Api apiClient,
        ApplicationDbContext dbContext
    )
    {
        var user = await FakerUtils.StoreFaker(dbContext, new AuthedUserFaker());
        var tokens = await Authenticate(apiClient, user, AuthedUserFaker.Password);
        return (user, tokens);
    }
}
