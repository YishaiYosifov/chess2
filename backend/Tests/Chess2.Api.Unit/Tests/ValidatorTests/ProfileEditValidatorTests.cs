using Chess2.Api.Models.DTOs;
using Chess2.Api.TestInfrastructure.TestData;
using Chess2.Api.Validators;
using FluentAssertions;

namespace Chess2.Api.Unit.Tests.ValidatorTests;

public class ProfileEditValidatorTests
{
    private readonly ProfileEditValidator _validator = new();

    private const string longAbout =
        @"Very long about me
        aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa
        aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa
        aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";

    [Theory]
    [InlineData("", true)]
    [InlineData(null, true)]
    [InlineData("abc", true)]
    [InlineData(longAbout, false)]
    public void Validate_about(string? about, bool isValid)
    {
        var profileEdit = new ProfileEditRequest() { About = about };
        _validator.Validate(profileEdit).IsValid.Should().Be(isValid);
    }

    [Theory]
    [ClassData(typeof(CountryCodeTestData))]
    public void Validate_country(string? country, bool isValid)
    {
        var profileEdit = new ProfileEditRequest() { CountryCode = country };
        _validator.Validate(profileEdit).IsValid.Should().Be(isValid);
    }
}
