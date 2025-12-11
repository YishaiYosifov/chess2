using System.Text.Json;

namespace AnarchyChess.Api.CountryCodes.Services;

public static class CountryCodeLookup
{
    private static readonly HashSet<string> _validCodes;

    static CountryCodeLookup()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Data", "countryCodes.json");
        var json = File.ReadAllText(path);
        _validCodes = JsonSerializer.Deserialize<HashSet<string>>(json) ?? [];
    }

    public static bool IsValid(string? code) =>
        code is not null && _validCodes.Contains(code.ToUpper());
}
