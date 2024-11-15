using FluentValidation;
using System.Globalization;
using System.Reflection;
using System.Resources;

namespace Chess2.Api.Validators;

public static class CountryCodeValidator
{
    public static IRuleBuilderOptions<T, string> MustBeACountryCode<T>(this IRuleBuilder<T, string> ruleBuilder, string? message = null)
    {
        message ??= "Must be a valid 2 letter country code";
        return ruleBuilder.Must(BeAValidCountryCode).WithMessage(message);
    }

    private static bool BeAValidCountryCode(string countryCode)
    {
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
