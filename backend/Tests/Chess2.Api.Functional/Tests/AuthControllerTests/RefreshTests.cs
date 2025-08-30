using System.Net;
using Chess2.Api.TestInfrastructure;
using Chess2.Api.TestInfrastructure.Fakes;
using Chess2.Api.TestInfrastructure.Utils;
using FluentAssertions;

namespace Chess2.Api.Functional.Tests.AuthControllerTests;

public class RefreshTests(Chess2WebApplicationFactory factory) : BaseFunctionalTest(factory)
{
    [Fact]
    public async Task RefreshToken_refreshes_and_sets_the_access_token()
    {
        // create an api client with just a refresh token, no access token
        await AuthUtils.AuthenticateAsync(ApiClient, setAccessToken: false);

        var response = await ApiClient.Api.RefreshTokenAsync();

        response.IsSuccessful.Should().BeTrue();
        await AuthTestUtils.AssertAuthenticated(ApiClient);
    }

    [Fact]
    public async Task RefreshToken_disallows_refreshing_when_provided_an_access_token()
    {
        var user = new AuthedUserFaker().Generate();
        await DbContext.AddAsync(user, CT);
        await DbContext.SaveChangesAsync(CT);

        var accessToken = TokenProvider.GenerateAccessToken(user);
        AuthUtils.AuthenticateWithTokens(ApiClient, refreshToken: accessToken);

        var response = await ApiClient.Api.RefreshTokenAsync();

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        await AuthTestUtils.AssertUnauthenticated(ApiClient);
    }

    [Fact]
    public async Task RefreshToken_disallows_refreshing_using_the_same_token_twice()
    {
        var refreshToken = (await AuthUtils.AuthenticateAsync(ApiClient)).RefreshToken;

        var firstRefreshResponse = await ApiClient.Api.RefreshTokenAsync();
        firstRefreshResponse.IsSuccessful.Should().BeTrue();

        AuthUtils.AuthenticateWithTokens(ApiClient, refreshToken: refreshToken);

        var secondRefreshResponse = await ApiClient.Api.RefreshTokenAsync();
        secondRefreshResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
