using Chess2.Api.Services;
using FluentAssertions;

namespace Chess2.Api.Unit.Tests;

public class PasswordHasherTests
{
    private readonly PasswordHasher _passwordHasher = new();

    [Fact]
    public async Task Password_is_hashed_correctly()
    {
        var password = "TestPassword";
        var salt = _passwordHasher.GenerateSalt();

        var hash = await _passwordHasher.HashPasswordAsync(password, salt);

        hash.Should().NotBeNullOrEmpty();
        hash.Should().HaveCount(32);
    }

    [Fact]
    public void Salt_generation_is_random()
    {
        var salt1 = _passwordHasher.GenerateSalt();
        var salt2 = _passwordHasher.GenerateSalt();

        salt1.Should().NotBeNullOrEmpty();
        salt2.Should().NotBeNullOrEmpty();

        salt1.Should().HaveCount(16);

        salt1.Should().NotBeEquivalentTo(salt2);
    }

    [Theory]
    [InlineData("TestPassword", "TestPassword")]
    [InlineData("TestPassword", "OtherTestPassword")]
    public async Task Correctly_verifies_password_with_hash_and_salt(
        string password1,
        string password2
    )
    {
        var salt = _passwordHasher.GenerateSalt();
        var hash = await _passwordHasher.HashPasswordAsync(password1, salt);

        var isVerified = await _passwordHasher.VerifyPassword(password2, hash, salt);

        isVerified.Should().Be(password1 == password2);
    }
}
