namespace AnarchyChess.Api.CountryCodes.Services;

public interface IHeaderCountryResolver
{
    string? GetCountryCode(HttpContext context);
}

public class HeaderCountryResolver : IHeaderCountryResolver
{
    public const string HeaderName = "CF-IPCountry";

    public string? GetCountryCode(HttpContext context)
    {
        var headerCountry = context.Request.Headers[HeaderName].ToString();
        return CountryCodeLookup.IsValid(headerCountry) ? headerCountry : null;
    }
}
