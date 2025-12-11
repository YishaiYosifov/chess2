using AnarchyChess.Api.CountryCodes.Services;
using AnarchyChess.Api.TestInfrastructure.TestData;
using AwesomeAssertions;

namespace AnarchyChess.Api.Unit.Tests.CountryCodesTests;

public class CountryCodeLookupTests
{
    [Theory]
    [ClassData(typeof(CountryCodeTestData))]
    public void IsValid_returns_expected_value(string country, bool isValid)
    {
        var result = CountryCodeLookup.IsValid(country);
        result.Should().Be(isValid);
    }
}
