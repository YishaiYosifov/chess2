using System.Net;
using Chess2.Api.Functional.Utils;
using Chess2.Api.TestInfrastructure;
using Chess2.Api.TestInfrastructure.Fakes;
using Chess2.Api.TestInfrastructure.Utils;
using FluentAssertions;

namespace Chess2.Api.Functional.Tests.AuthControllerTests;

public class LoginTests(Chess2WebApplicationFactory factory) : BaseFunctionalTest(factory)
{
    [Theory]
    [InlineData("TestUsername", "test@email.com", "TestUsername")]
    [InlineData("TestUsername", "test@email.com", "test@email.com")]
    [InlineData("TestUsername", "test@email.com", "tEsT@eMaIl.cOm")]
    [InlineData("TestUsername", "test@email.com", "tEsTuSeRnAmE")]
    public async Task Login_with_correct_credentials(
        string username,
        string email,
        string loginWithIdentifier
    )
    {
        var user = await FakerUtils.StoreFaker(
            DbContext,
            new AuthedUserFaker().RuleFor(x => x.UserName, username).RuleFor(x => x.Email, email)
        );

        var response = await ApiClient.SigninAsync(
            new() { UsernameOrEmail = loginWithIdentifier, Password = AuthedUserFaker.Password }
        );

        response.IsSuccessful.Should().BeTrue();
        var cookies = response.Headers.GetValues("Set-Cookie");
        cookies.Should().HaveCount(2);
        await AuthTestUtils.AssertAuthenticated(ApiClient);
    }

    [Fact]
    public async Task Login_with_non_existing_user()
    {
        await FakerUtils.StoreFaker(DbContext, new AuthedUserFaker());

        var response = await ApiClient.SigninAsync(
            new()
            {
                UsernameOrEmail = "random email or username doesn't exist",
                Password = AuthedUserFaker.Password,
            }
        );

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        await AuthTestUtils.AssertUnauthenticated(ApiClient);
    }

    [Fact]
    public async Task Login_with_wrong_password()
    {
        var user = await FakerUtils.StoreFaker(DbContext, new AuthedUserFaker());

        var response = await ApiClient.SigninAsync(
            new() { UsernameOrEmail = user.UserName!, Password = "wrong password" }
        );

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        await AuthTestUtils.AssertUnauthenticated(ApiClient);
    }
}
