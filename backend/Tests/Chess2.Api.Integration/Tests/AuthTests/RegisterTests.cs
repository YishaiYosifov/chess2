using Chess2.Api.Integration.Collections;
using Chess2.Api.Integration.Fakes;
using Chess2.Api.Models.DTOs;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace Chess2.Api.Integration.Tests.AuthTests;

public class RegisterTests(Chess2WebApplicationFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task User_registration_saves_user_to_database()
    {
        var userIn = new UserIn()
        {
            Username = "TestUser",
            Email = "test@email.com",
            Password = "TestPassword",
        };
        var response = await ApiClient.RegisterAsync(userIn);
        var registeredUser = response.Content;
        var allUsers = await DbContext.Users.ToListAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        registeredUser.Should().NotBeNull();
        (registeredUser?.Username).Should().Be(userIn.Username);
        (registeredUser?.Email).Should().Be(userIn.Email);

        allUsers.Should().HaveCount(1);
        var dbUser = allUsers.First();
        dbUser.Username.Should().Be(userIn.Username);
        dbUser.Email.Should().Be(userIn.Email);
        dbUser.UserId.Should().Be(registeredUser?.UserId);
    }

    [Theory]
    [InlineData("", "test@email.com", "TestPassword")]
    [InlineData("LongUsernameeeeeeeeeeeeeeeeeeee", "test@email.com", "TestPassword")]
    [InlineData("TestUser", "bad-email", "TestPassword")]
    [InlineData("TestUser", "test@email.com", "")]
    [InlineData("TestUser", "test@email.com", "ShtPwd")]
    public async Task Invalid_parameters_returns_error(string username, string email, string password)
    {
        var userIn = new UserIn()
        {
            Username = username,
            Email = email,
            Password = password,
        };
        var response = await ApiClient.RegisterAsync(userIn);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        response.Content.Should().BeNull();
        DbContext.Users.Should().HaveCount(0);
    }

    [Theory]
    [InlineData("TestUsername", "test@email.com", "TestUsername", "other-test@email.com")]
    [InlineData("TestUsername", "test@email.com", "OtherTestUsername", "test@email.com")]
    public async Task Conflicting_parameters_with_other_users_returns_error(
        string user1Username,
        string user1Email,
        string user2Username,
        string user2Email)
    {
        var user1 = new UserFaker()
            .RuleFor(x => x.Username, user1Username)
            .RuleFor(x => x.Email, user1Email)
            .Generate();
        await DbContext.Users.AddAsync(user1);
        await DbContext.SaveChangesAsync();

        var response = await ApiClient.RegisterAsync(new()
        {
            Username = user2Username,
            Email = user2Email,
            Password = "TestPassword"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        response.Content.Should().BeNull();
        DbContext.Users.Should().HaveCount(1);
    }
}
