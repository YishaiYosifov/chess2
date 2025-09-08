using Chess2.Api.Profile.Entities;
using Chess2.Api.Profile.Services;
using Chess2.Api.Shared.Services;
using Chess2.Api.TestInfrastructure.Fakes;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using NSubstitute;

namespace Chess2.Api.Unit.Tests;

public class UsernameGeneratorTests : BaseUnitTest
{
    private readonly UserManager<AuthedUser> _userManagerMock;
    private readonly IUsernameWordsProvider _usernameWordsProviderMock =
        Substitute.For<IUsernameWordsProvider>();
    private readonly IRandomProvider _randomMock = Substitute.For<IRandomProvider>();

    private readonly UsernameGenerator _usernameGenerator;

    private readonly List<string> _adjectives = ["Cool", "Awesome"];
    private readonly List<string> _nouns = ["Cat", "Dog", "Horse"];

    public UsernameGeneratorTests()
    {
        _userManagerMock = Substitute.ForPartsOf<UserManager<AuthedUser>>(
            Substitute.For<IUserStore<AuthedUser>>(),
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null
        );
        _usernameGenerator = new UsernameGenerator(
            _usernameWordsProviderMock,
            _userManagerMock,
            _randomMock
        );

        _usernameWordsProviderMock.Adjectives.Returns(_adjectives);
        _usernameWordsProviderMock.Nouns.Returns(_nouns);
    }

    [Fact]
    public async Task GenerateUniqueUsernameAsync_returns_a_random_username()
    {
        var expectedUsername = $"{_adjectives[0]}-{_nouns[1]}-1234";

        _randomMock.NextItem(_adjectives).Returns(_adjectives[0]);
        _randomMock.NextItem(_nouns).Returns(_nouns[1]);
        _randomMock.Next(1000, 10000).Returns(1234);

        _userManagerMock.FindByNameAsync(expectedUsername).Returns((AuthedUser?)null);

        var result = await _usernameGenerator.GenerateUniqueUsernameAsync();

        result.Should().Be(expectedUsername);
    }

    [Fact]
    public async Task GenerateUniqueUsernameAsync_retries_if_the_username_is_taken()
    {
        var takenUsername = $"{_adjectives[0]}-{_nouns[0]}-1111";
        var expectedUsername = $"{_adjectives[1]}-{_nouns[1]}-2222";

        _randomMock.NextItem(_adjectives).Returns(_adjectives[0], _adjectives[1]);
        _randomMock.NextItem(_nouns).Returns(_nouns[0], _nouns[1]);
        _randomMock.Next(1000, 10000).Returns(1111, 2222);

        var existingUser = new AuthedUserFaker().RuleFor(x => x.UserName, takenUsername).Generate();
        _userManagerMock.FindByNameAsync(takenUsername).Returns(existingUser);

        var result = await _usernameGenerator.GenerateUniqueUsernameAsync();

        result.Should().Be(expectedUsername);
    }
}
