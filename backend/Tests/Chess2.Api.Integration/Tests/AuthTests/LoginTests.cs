using Chess2.Api.Integration.Fakes;
using Chess2.Api.Integration.Utils;
using FluentAssertions;
using System.Net;

namespace Chess2.Api.Integration.Tests.AuthTests;

public class LoginTests(Chess2WebApplicationFactory factory) : BaseIntegrationTest(factory)
{
    [Theory]
    [InlineData("TestUsername", "test@email.com", "TestUsername")]
    [InlineData("TestUsername", "test@email.com", "test@email.com")]
    public async Task Login_with_existing_user(
        string username,
        string email,
        string loginWithIdentifier)
    {
        var user = await FakerUtils.StoreFaker(
            DbContext, new UserFaker().RuleFor(x => x.Username, username)
            .RuleFor(x => x.Email, email));

        var response = await ApiClient.LoginAsync(new()
        {
            UsernameOrEmail = loginWithIdentifier,
            Password = UserFaker.Password,
        });

        response.IsSuccessful.Should().BeTrue();
        var cookies = response.Headers.GetValues("Set-Cookie");
        cookies.Should().HaveCount(2);
        await AuthTestUtils.AssertHttpClientAuthenticated(ApiClient);
    }

    [Fact]
    public async Task Login_with_bad_credentials()
    {
        await FakerUtils.StoreFaker(DbContext, new UserFaker());

        var response = await ApiClient.LoginAsync(new()
        {
            UsernameOrEmail = "random email or username doesn't exist",
            Password = UserFaker.Password,
        });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        await AuthTestUtils.AssertHttpClientUnauthenticated(ApiClient);
    }

    [Fact]
    public async Task Login_with_wrong_password()
    {
        var user = await FakerUtils.StoreFaker(DbContext, new UserFaker());

        var response = await ApiClient.LoginAsync(new()
        {
            UsernameOrEmail = user.Username,
            Password = "wrong password",
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        await AuthTestUtils.AssertHttpClientUnauthenticated(ApiClient);
    }
}
