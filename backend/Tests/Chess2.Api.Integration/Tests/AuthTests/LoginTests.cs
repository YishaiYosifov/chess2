using Chess2.Api.Integration.Fakes;
using FluentAssertions;
using System.Net;

namespace Chess2.Api.Integration.Tests.AuthTests;

public class LoginTests(Chess2WebApplicationFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task Login_with_existing_user()
    {
        var user = new UserFaker().Generate();
        await DbContext.Users.AddAsync(user);
        await DbContext.SaveChangesAsync();

        var response = await ApiClient.LoginAsync(new()
        {
            UsernameOrEmail = user.Email,
            Password = UserFaker.Password,
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var cookies = response.Headers.GetValues("Set-Cookie");
        cookies.Should().HaveCount(2);

        var testAuthResponse = await ApiClient.TestAuthAsync();
        testAuthResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Login_with_wrong_password()
    {
        var user = new UserFaker().Generate();
        await DbContext.Users.AddAsync(user);
        await DbContext.SaveChangesAsync();

        var response = await ApiClient.LoginAsync(new()
        {
            UsernameOrEmail = user.Email,
            Password = "Wrong password",
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        var testAuthResponse = await ApiClient.TestAuthAsync();
        testAuthResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
