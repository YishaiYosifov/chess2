using IP2Location;
using IPDatabase = IP2Location.Component;

namespace AnarchyChess.Api.Auth.Services;

public interface ICountryResolver
{
    Task<string?> LocateAsync(string? ip);
}

public class CountryResolver : ICountryResolver, IDisposable
{
    private readonly ILogger<CountryResolver> _logger;

    private readonly IPDatabase _ipDb = new();

    public CountryResolver(ILogger<CountryResolver> logger)
    {
        _logger = logger;

        var path = Path.Combine(AppContext.BaseDirectory, "Data", "IP2LOCATION-LITE-DB1.IPV6.BIN");
        _ipDb.Open(path, UseMMF: true);
    }

    public async Task<string?> LocateAsync(string? ip)
    {
        if (ip is null)
            return null;

        IPResult result;
        try
        {
            result = await _ipDb.IPQueryAsync(ip);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to query IP2Location database for ip {IP}", ip);
            return null;
        }

        if (result.Status != "OK")
        {
            _logger.LogWarning("Could not locate country by ip {IP}, {Status}", ip, result.Status);
            return null;
        }

        var alpha2 = result.CountryShort;
        if (alpha2.Length != 2)
        {
            _logger.LogWarning(
                "Could not locate country by ip {IP}, result was {Result}",
                ip,
                alpha2
            );
            return null;
        }

        return alpha2;
    }

    public void Dispose()
    {
        _ipDb.Close();
        GC.SuppressFinalize(this);
    }
}
