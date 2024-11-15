using Chess2.Api.Functional.Utils;
using Chess2.Api.Models.DTOs;
using Chess2.Api.TestInfrastructure;
using Chess2.Api.TestInfrastructure.Fakes;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace Chess2.Api.Functional.Tests.AuthTests;

public class RegisterTests(Chess2WebApplicationFactory factory) : BaseFunctionalTest(factory)
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

        response.IsSuccessful.Should().BeTrue();
        registeredUser.Should().NotBeNull();
        registeredUser.Should().BeEquivalentTo(
            userIn, opts => opts.ExcludingMissingMembers());

        allUsers.Should().HaveCount(1);
        var dbUser = allUsers.First();
        registeredUser.Should().BeEquivalentTo(
            dbUser, opts => opts.ExcludingMissingMembers());
    }

    [Theory]
    [InlineData("", "test@email.com", "TestPassword")]
    [InlineData("LongUsernameeeeeeeeeeeeeeeeeeee", "test@email.com", "TestPassword")]
    [InlineData("TestUser", "bad-email", "TestPassword")]
    [InlineData("TestUser", "test@email.com", "")]
    [InlineData("TestUser", "test@email.com", "ShtPwd")]
    public async Task Register_with_bad_parameters(string username, string email, string password)
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
    public async Task Register_with_conflicting_credentials_with_another_user(
        string user1Username,
        string user1Email,
        string user2Username,
        string user2Email)
    {
        var user1 = await FakerUtils.StoreFaker(
            DbContext, new UserFaker().RuleFor(x => x.Username, user1Username)
            .RuleFor(x => x.Email, user1Email));

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
