using Chess2.Api.Functional.Utils;
using Chess2.Api.TestInfrastructure;
using Chess2.Api.TestInfrastructure.Fakes;
using FluentAssertions;
using System.Net;

namespace Chess2.Api.Functional.Tests.AuthTests;

public class RefreshTests(Chess2WebApplicationFactory factory) : BaseFunctionalTest(factory)
{
    [Fact]
    public async Task Refresh_with_a_valid_refresh_token()
    {
        var tokens = (await AuthTestUtils.Authenticate(ApiClient, DbContext)).Tokens;

        var refreshTokenClient = Factory.CreateTypedClientWithTokens(refreshToken: tokens.RefreshToken);
        var response = await refreshTokenClient.RefreshTokenAsync();

        response.IsSuccessful.Should().BeTrue();
        await AuthTestUtils.AssertAuthenticated(refreshTokenClient);
    }

    [Fact]
    public async Task Refresh_with_access_instead_of_refresh_token()
    {
        var tokens = (await AuthTestUtils.Authenticate(ApiClient, DbContext)).Tokens;

        var accessTokenClient = Factory.CreateTypedClientWithTokens(refreshToken: tokens.AccessToken);
        var response = await accessTokenClient.RefreshTokenAsync();

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        await AuthTestUtils.AssertUnauthenticated(accessTokenClient);
    }

    [Fact]
    public async Task Refresh_after_password_change()
    {
        var passwordChanged = DateTime.UtcNow.AddSeconds(2);
        var user = await FakerUtils.StoreFaker(
            DbContext, new UserFaker().RuleFor(x => x.PasswordLastChanged, passwordChanged));
        var tokens = await AuthTestUtils.Authenticate(ApiClient, user, UserFaker.Password);

        var refreshTokenClient = Factory.CreateTypedClientWithTokens(refreshToken: tokens.RefreshToken);
        var response = await ApiClient.RefreshTokenAsync();

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        await AuthTestUtils.AssertUnauthenticated(refreshTokenClient);
    }
}
