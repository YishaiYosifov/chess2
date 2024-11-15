using Chess2.Api.Repositories;
using Chess2.Api.TestInfrastructure;
using Chess2.Api.TestInfrastructure.Fakes;
using Chess2.Api.TestInfrastructure.Utils;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Chess2.Api.Integration.Tests;

public class UserRepositoryTests : BaseIntegrationTest
{
    private readonly IUserRepository _userRepository;

    public UserRepositoryTests(Chess2WebApplicationFactory factory) : base(factory)
    {
        _userRepository = Scope.ServiceProvider.GetRequiredService<IUserRepository>();
    }

    [Theory]
    [InlineData("TestUsername", "TestUsername", true)]
    [InlineData("TestUsername", "OtherTestUsername", false)]
    public async Task Get_user_with_correct_and_incorrect_usernames(string username, string getUsername, bool shouldSucceed)
    {
        var user = await FakerUtils.StoreFaker(
            DbContext, new UserFaker().RuleFor(x => x.Username, username));

        var fetchedUser = await _userRepository.GetByUsernameAsync(getUsername);

        if (shouldSucceed) fetchedUser.Should().BeEquivalentTo(user);
        else fetchedUser.Should().BeNull();
    }

    [Theory]
    [InlineData("test@email.com", "test@email.com", true)]
    [InlineData("test@email.com", "othertest@email.com", false)]
    public async Task Get_user_with_correct_and_incorrect_emails(string email, string getEmail, bool shouldSucceed)
    {
        var user = await FakerUtils.StoreFaker(
            DbContext, new UserFaker().RuleFor(x => x.Email, email));

        var fetchedUser = await _userRepository.GetByEmailAsync(getEmail);

        if (shouldSucceed) fetchedUser.Should().BeEquivalentTo(user);
        else fetchedUser.Should().BeNull();
    }

    [Fact]
    public async Task Create_a_user()
    {
        var user = new UserFaker().Generate();
        await _userRepository.AddUserAsync(user);

        var users = await DbContext.Users.ToListAsync();
        users.Should().HaveCount(1);
        users.First().Should().BeEquivalentTo(user);
    }
}
