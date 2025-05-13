using System.Net;
using Chess2.Api.TestInfrastructure;
using Chess2.Api.TestInfrastructure.Fakes;
using Chess2.Api.TestInfrastructure.Utils;
using FluentAssertions;

namespace Chess2.Api.Functional.Tests.AuthControllerTests;

public class RefreshTests(Chess2WebApplicationFactory factory) : BaseFunctionalTest(factory)
{
    [Fact]
    public async Task Refresh_with_a_valid_refresh_token()
    {
        // create an api client with just a refresh token, no access token
        await AuthUtils.AuthenticateAsync(ApiClient, setAccessToken: false);

        var response = await ApiClient.Api.RefreshTokenAsync();

        response.IsSuccessful.Should().BeTrue();
        await AuthUtils.AssertAuthenticated(ApiClient);
    }

    [Fact]
    public async Task Refresh_with_access_instead_of_refresh_token()
    {
        var user = await FakerUtils.StoreFakerAsync(DbContext, new AuthedUserFaker());
        var accessToken = TokenProvider.GenerateAccessToken(user);
        AuthUtils.AuthenticateWithTokens(ApiClient, refreshToken: accessToken);

        var response = await ApiClient.Api.RefreshTokenAsync();

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        await AuthUtils.AssertUnauthenticated(ApiClient);
    }

    [Fact]
    public async Task Refresh_after_password_change()
    {
        var passwordChanged = DateTime.UtcNow.AddSeconds(2);
        var user = await FakerUtils.StoreFakerAsync(
            DbContext,
            new AuthedUserFaker().RuleFor(x => x.PasswordLastChanged, passwordChanged)
        );
        AuthUtils.AuthenticateWithUser(ApiClient, user, setAccessToken: false);

        var response = await ApiClient.Api.RefreshTokenAsync();

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        await AuthUtils.AssertUnauthenticated(ApiClient);
    }
}
