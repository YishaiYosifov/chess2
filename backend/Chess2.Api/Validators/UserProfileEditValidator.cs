using Chess2.Api.Models.DTOs;
using FluentValidation;

namespace Chess2.Api.Validators;

public class UserProfileEditValidator : AbstractValidator<UserProfileUpdate>
{
    public UserProfileEditValidator()
    {
        RuleFor(x => x.About).MaximumLength(300);
        RuleFor(x => x.CountryCode)
            .MustBeACountryCode()
            .When(user => user.CountryCode is not null);
    }
}
