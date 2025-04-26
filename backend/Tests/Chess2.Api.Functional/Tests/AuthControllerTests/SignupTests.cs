using System.Net;
using Chess2.Api.Models.DTOs;
using Chess2.Api.TestInfrastructure;
using Chess2.Api.TestInfrastructure.Fakes;
using Chess2.Api.TestInfrastructure.Utils;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Chess2.Api.Functional.Tests.AuthControllerTests;

public class SignupTests(Chess2WebApplicationFactory factory) : BaseFunctionalTest(factory)
{
    [Fact]
    public async Task User_signup_saves_user_to_database()
    {
        var userIn = new SignupRequest()
        {
            Username = "TestUser",
            Email = "test@email.com",
            Password = "TestPassword",
            CountryCode = "IL",
        };
        var response = await ApiClient.SignupAsync(userIn);
        var registeredUser = response.Content;
        var allUsers = await DbContext.Users.ToListAsync();

        response.IsSuccessful.Should().BeTrue();
        registeredUser.Should().NotBeNull();
        registeredUser.Should().BeEquivalentTo(userIn, opts => opts.ExcludingMissingMembers());

        allUsers.Should().HaveCount(1);
        var dbUser = allUsers.First();
        registeredUser.Should().BeEquivalentTo(dbUser, opts => opts.ExcludingMissingMembers());
    }

    [Theory]
    [InlineData("", "test@email.com", "TestPassword", "IL")]
    [InlineData("LongUsernameeeeeeeeeeeeeeeeeeee", "test@email.com", "TestPassword", "IL")]
    [InlineData("TestUser", "bad-email", "TestPassword", "IL")]
    [InlineData("TestUser", "test@email.com", "", "IL")]
    [InlineData("TestUser", "test@email.com", "ShtPwd", "IL")]
    [InlineData("TestUser", "test@email.com", "ShtPwd", "XZ")]
    public async Task Signup_with_bad_parameters(
        string username,
        string email,
        string password,
        string country
    )
    {
        var userIn = new SignupRequest()
        {
            Username = username,
            Email = email,
            Password = password,
            CountryCode = country,
        };
        var response = await ApiClient.SignupAsync(userIn);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        response.Content.Should().BeNull();
        DbContext.Users.Should().HaveCount(0);
    }

    [Theory]
    [InlineData("TestUsername", "test@email.com", "TestUsername", "other-test@email.com")]
    [InlineData("TestUsername", "test@email.com", "OtherTestUsername", "test@email.com")]
    public async Task Signup_with_conflicting_credentials_with_another_user(
        string user1Username,
        string user1Email,
        string user2Username,
        string user2Email
    )
    {
        var user1 = await FakerUtils.StoreFaker(
            DbContext,
            new AuthedUserFaker()
                .RuleFor(x => x.Username, user1Username)
                .RuleFor(x => x.Email, user1Email)
        );

        var response = await ApiClient.SignupAsync(
            new()
            {
                Username = user2Username,
                Email = user2Email,
                Password = "TestPassword",
                CountryCode = "IL",
            }
        );

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        response.Content.Should().BeNull();
        DbContext.Users.Should().HaveCount(1);
    }
}
