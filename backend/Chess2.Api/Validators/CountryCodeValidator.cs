using System.Globalization;
using FluentValidation;

namespace Chess2.Api.Validators;

public static class CountryCodeValidator
{
    /// <summary>
    /// Verifies a string is a valid 2 character country code
    /// </summary>
    public static IRuleBuilderOptions<T, string?> MustBeACountryCode<T>(
        this IRuleBuilder<T, string?> ruleBuilder,
        string? message = null
    )
    {
        message ??= "Must be a valid 2 letter country code";
        return ruleBuilder.Must(BeAValidCountryCode).WithMessage(message);
    }

    private static bool BeAValidCountryCode(string? countryCode)
    {
        if (string.IsNullOrWhiteSpace(countryCode))
            return false;

        try
        {
            var info = new RegionInfo(countryCode);
            return info is not null;
        }
        catch
        {
            return false;
        }
    }
}
