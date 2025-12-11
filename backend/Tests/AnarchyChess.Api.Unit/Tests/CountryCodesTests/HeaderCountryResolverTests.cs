using AnarchyChess.Api.CountryCodes.Services;
using AnarchyChess.Api.TestInfrastructure.TestData;
using AwesomeAssertions;
using Microsoft.AspNetCore.Http;

namespace AnarchyChess.Api.Unit.Tests.CountryCodesTests;

public class HeaderCountryResolverTests
{
    private readonly HeaderCountryResolver _resolver = new();

    private const string ExpectedHeader = "CF-IPCountry";

    [Theory]
    [ClassData(typeof(CountryCodeTestData))]
    public void GetCountryCode_only_returns_valid_codes(string country, bool isValid)
    {
        DefaultHttpContext context = new();
        context.Request.Headers.TryAdd(ExpectedHeader, country);

        var result = _resolver.GetCountryCode(context);

        if (isValid)
            result.Should().Be(country);
        else
            result.Should().BeNull();
    }
}
