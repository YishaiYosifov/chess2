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
        await AuthUtils.AssertAuthenticated(ApiClient);
    }

    [Fact]
    public async Task RefreshToken_disallows_refreshing_when_provided_an_access_token()
    {
        var user = await FakerUtils.StoreFakerAsync(DbContext, new AuthedUserFaker());
        var accessToken = TokenProvider.GenerateAccessToken(user);
        AuthUtils.AuthenticateWithTokens(ApiClient, refreshToken: accessToken);

        var response = await ApiClient.Api.RefreshTokenAsync();

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        await AuthUtils.AssertUnauthenticated(ApiClient);
    }
}
