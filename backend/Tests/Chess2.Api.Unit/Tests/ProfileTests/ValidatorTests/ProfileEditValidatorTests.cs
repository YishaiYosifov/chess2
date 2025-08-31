using Chess2.Api.Profile.DTOs;
using Chess2.Api.Profile.Validators;
using Chess2.Api.TestInfrastructure.TestData;
using FluentValidation.TestHelper;

namespace Chess2.Api.Unit.Tests.ProfileTests.ValidatorTests;

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
    [InlineData("")]
    [InlineData("abc")]
    public void ProfileEditValidator_accepts_about(string about)
    {
        ProfileEditRequest model = new(About: about, CountryCode: "XX");
        var result = _validator.TestValidate(model);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void ProfileEditValidator_rejects_long_about()
    {
        ProfileEditRequest model = new(About: longAbout, CountryCode: "XX");
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.About);
    }

    [Theory]
    [ClassData(typeof(CountryCodeTestData))]
    public void ProfileEditValidator_validates_country(string country, bool isValid)
    {
        ProfileEditRequest model = new(About: "", CountryCode: country);
        var result = _validator.TestValidate(model);

        if (isValid)
            result.ShouldNotHaveAnyValidationErrors();
        else
            result.ShouldHaveValidationErrorFor(x => x.CountryCode);
    }
}
