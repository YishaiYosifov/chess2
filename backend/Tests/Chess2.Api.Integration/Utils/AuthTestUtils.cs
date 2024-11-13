using FluentAssertions;
using Refit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Chess2.Api.Integration.Utils;

public static class AuthTestUtils
{
    /// <summary>
    /// Calls <see cref="IsHttpClientAuthenticated(IChess2Api)"/> and asserts it is successful
    /// </summary>
    public static async Task AssertHttpClientAuthenticated(IChess2Api apiClient)
    {
        (await IsHttpClientAuthenticated(apiClient)).Should().BeTrue();
    }

    /// <summary>
    /// Calls <see cref="IsHttpClientAuthenticated(IChess2Api)"/> and asserts it is not successful
    /// </summary>
    public static async Task AssertHttpClientUnauthenticated(IChess2Api apiClient)
    {
        (await IsHttpClientAuthenticated(apiClient)).Should().BeFalse();
    }

    /// <summary>
    /// Attempt to call a test route to check if we are authenticated
    /// </summary>
    public static async Task<bool> IsHttpClientAuthenticated(IChess2Api apiClient)
    {
        var testAuthResponse = await apiClient.TestAuthAsync();
        return testAuthResponse.StatusCode == HttpStatusCode.NoContent;
    }
}
