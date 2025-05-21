using Chess2.Api.Users.DTOs;
using FluentValidation;

namespace Chess2.Api.Users.Validators;

public class ProfileEditValidator : AbstractValidator<ProfileEditRequest>
{
    public ProfileEditValidator()
    {
        RuleFor(x => x.About).MaximumLength(300);
        RuleFor(x => x.CountryCode).MustBeACountryCode().When(user => user.CountryCode is not null);
    }
}
