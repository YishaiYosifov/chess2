using Chess2.Api.TestInfrastructure.Fakes;
using Chess2.Api.Validators;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess2.Api.Unit.Tests.ValidatorTests;

public class UserValidatorTests
{
    private readonly UserValidator _validator = new();

    [Theory]
    [InlineData("", false)]
    [InlineData(" ", false)]
    [InlineData("usernameeeeeeeeeeeeeeeeeeeeeeee", false)]
    [InlineData("TestUsername", true)]
    public void Validate_username(string username, bool isValid)
    {
        var userIn = new UserInFaker().RuleFor(x => x.Username, username).Generate();
        _validator.Validate(userIn).IsValid.Should().Be(isValid);
    }

    [Theory]
    [InlineData("a@", false)]
    [InlineData("a@b.c", true)]
    public void Validate_email(string email, bool isValid)
    {
        var userIn = new UserInFaker().RuleFor(x => x.Email, email).Generate();
        _validator.Validate(userIn).IsValid.Should().Be(isValid);
    }

    [Theory]
    [InlineData("123", false)]
    [InlineData("12345678", true)]
    public void Validate_password(string password, bool isValid)
    {
        var userIn = new UserInFaker().RuleFor(x => x.Password, password).Generate();
        _validator.Validate(userIn).IsValid.Should().Be(isValid);
    }

    [Theory]
    [InlineData("XZ", false)]
    [InlineData("IL", true)]
    [InlineData(null, true)]
    public void Validate_country(string? country, bool isValid)
    {
        var userIn = new UserInFaker().RuleFor(x => x.CountryCode, country).Generate();
        _validator.Validate(userIn).IsValid.Should().Be(isValid);
    }
}
