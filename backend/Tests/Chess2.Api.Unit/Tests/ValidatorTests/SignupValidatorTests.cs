using Chess2.Api.TestInfrastructure.Fakes;
using Chess2.Api.TestInfrastructure.TestData;
using Chess2.Api.Validators;
using FluentAssertions;

namespace Chess2.Api.Unit.Tests.ValidatorTests;

public class SignupValidatorTests
{
    private readonly SignupValidator _validator = new();

    [Theory]
    [InlineData("", false)]
    [InlineData(" ", false)]
    [InlineData("usernameeeeeeeeeeeeeeeeeeeeeeee", false)]
    [InlineData("TestUsername", true)]
    public void Validate_username(string username, bool isValid)
    {
        var userIn = new SignupRequestFaker().RuleFor(x => x.UserName, username).Generate();
        _validator.Validate(userIn).IsValid.Should().Be(isValid);
    }

    [Theory]
    [InlineData("a@", false)]
    [InlineData("a@b.c", true)]
    public void Validate_email(string email, bool isValid)
    {
        var userIn = new SignupRequestFaker().RuleFor(x => x.Email, email).Generate();
        _validator.Validate(userIn).IsValid.Should().Be(isValid);
    }

    [Theory]
    [InlineData("123", false)]
    [InlineData("12345678", true)]
    public void Validate_password(string password, bool isValid)
    {
        var userIn = new SignupRequestFaker().RuleFor(x => x.Password, password).Generate();
        _validator.Validate(userIn).IsValid.Should().Be(isValid);
    }

    [Theory]
    [ClassData(typeof(CountryCodeTestData))]
    public void Validate_country(string? country, bool isValid)
    {
        var userIn = new SignupRequestFaker().RuleFor(x => x.CountryCode, country).Generate();
        _validator.Validate(userIn).IsValid.Should().Be(isValid);
    }
}
