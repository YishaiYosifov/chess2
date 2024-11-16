using Chess2.Api.Validators;
using FluentAssertions;
using FluentValidation;

namespace Chess2.Api.Unit.Tests.ValidatorTests;

public class CountryCodeValidatorTests
{
    private readonly InlineValidator<string?> _testValidator = [];
    public CountryCodeValidatorTests()
    {
        _testValidator.RuleFor(x => x).MustBeACountryCode();
    }

    [Theory]
    [InlineData("US")]
    [InlineData("GB")]
    [InlineData("CA")]
    [InlineData("IL")]
    public void Validate_a_valid_country_code(string countryCode)
    {
        var results = _testValidator.Validate(countryCode);
        results.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("ZZ")]
    [InlineData("U")]
    [InlineData("USA")]
    [InlineData("")]
    [InlineData("  ")]
    public void Validate_an_invalid_country_code(string? countryCode)
    {
        var results = _testValidator.Validate(countryCode);
        results.IsValid.Should().BeFalse();
    }
}
