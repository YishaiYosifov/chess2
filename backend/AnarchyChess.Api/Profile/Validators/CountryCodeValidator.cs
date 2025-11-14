using System.Text.Json;
using FluentValidation;

namespace AnarchyChess.Api.Profile.Validators;

public static class CountryCodeValidator
{
    private static readonly HashSet<string> ValidCodes;

    static CountryCodeValidator()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Data", "countryCodes.json");
        var json = File.ReadAllText(path);
        ValidCodes = JsonSerializer.Deserialize<HashSet<string>>(json) ?? [];
    }

    public static IRuleBuilderOptions<T, string?> MustBeACountryCode<T>(
        this IRuleBuilder<T, string?> ruleBuilder,
        string? message = null
    )
    {
        message ??= "Must be a valid iso alpha-2 country code";
        return ruleBuilder
            .NotEmpty()
            .NotNull()
            .Must(code => ValidCodes.Contains(code!.ToUpper()))
            .WithMessage(message);
    }
}
