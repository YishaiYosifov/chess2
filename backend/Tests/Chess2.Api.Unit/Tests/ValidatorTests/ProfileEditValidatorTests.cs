using Chess2.Api.TestInfrastructure.TestData;
using Chess2.Api.Users.DTOs;
using Chess2.Api.Users.Validators;
using FluentAssertions;

namespace Chess2.Api.Unit.Tests.ValidatorTests;

public class ProfileEditValidatorTests
{
    private readonly ProfileEditValidator _validator = new();

    private const string longAbout =
        @"Very long about me
        aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa
        aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa
        aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa
        aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa
        aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";

    [Theory]
    [InlineData("", true)]
    [InlineData("abc", true)]
    [InlineData(longAbout, false)]
    public void Validate_about(string about, bool isValid)
    {
        var profileEdit = new ProfileEditRequest(About: about, CountryCode: "XX");
        _validator.Validate(profileEdit).IsValid.Should().Be(isValid);
    }

    [Theory]
    [ClassData(typeof(CountryCodeTestData))]
    public void Validate_country(string country, bool isValid)
    {
        var profileEdit = new ProfileEditRequest(About: "", CountryCode: country);
        _validator.Validate(profileEdit).IsValid.Should().Be(isValid);
    }
}
