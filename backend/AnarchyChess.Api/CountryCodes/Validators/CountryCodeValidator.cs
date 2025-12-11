using AnarchyChess.Api.CountryCodes.Services;
using FluentValidation;

namespace AnarchyChess.Api.CountryCodes.Validators;

public static class CountryCodeValidator
{
    public static IRuleBuilderOptions<T, string?> MustBeACountryCode<T>(
        this IRuleBuilder<T, string?> ruleBuilder,
        string? message = null
    )
    {
        message ??= "Must be a valid iso alpha-2 country code";
        return ruleBuilder
            .NotEmpty()
            .NotNull()
            .Must(CountryCodeLookup.IsValid)
            .WithMessage(message);
    }
}
