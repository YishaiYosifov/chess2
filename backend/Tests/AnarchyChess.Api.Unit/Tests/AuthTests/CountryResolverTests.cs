using AnarchyChess.Api.Auth.Services;
using AwesomeAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace AnarchyChess.Api.Unit.Tests.AuthTests;

public class CountryResolverTests : IDisposable
{
    private readonly CountryResolver _resolver = new(Substitute.For<ILogger<CountryResolver>>());

    [Fact]
    public async Task LocateAsync_ReturnsValidCountry_ForKnownIp()
    {
        var country = await _resolver.LocateAsync("8.8.8.8");
        country.Should().Be("US");
    }

    [Fact]
    public async Task LocateAsync_returns_null_for_a_null_ip()
    {
        var result = await _resolver.LocateAsync(null);
        result.Should().BeNull();
    }

    [Fact]
    public async Task LocateAsync_returns_null_for_an_invalid_ip()
    {
        var result = await _resolver.LocateAsync("999.999.999.999");
        result.Should().BeNull();
    }

    [Fact]
    public async Task LocateAsync_returns_null_for_a_private_ip()
    {
        var result = await _resolver.LocateAsync("192.168.0.1");
        result.Should().BeNull();
    }

    public void Dispose()
    {
        _resolver.Dispose();
        GC.SuppressFinalize(this);
    }
}
