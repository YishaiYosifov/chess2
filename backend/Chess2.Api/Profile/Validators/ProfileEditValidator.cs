using Chess2.Api.Profile.DTOs;
using FluentValidation;

namespace Chess2.Api.Profile.Validators;

public class ProfileEditValidator : AbstractValidator<ProfileEditRequest>
{
    public ProfileEditValidator()
    {
        RuleFor(x => x.About).MaximumLength(500);
        RuleFor(x => x.CountryCode).MustBeACountryCode();
    }
}
