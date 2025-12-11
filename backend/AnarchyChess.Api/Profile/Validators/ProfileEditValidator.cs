using AnarchyChess.Api.CountryCodes.Validators;
using AnarchyChess.Api.Profile.DTOs;
using FluentValidation;

namespace AnarchyChess.Api.Profile.Validators;

public class ProfileEditValidator : AbstractValidator<ProfileEditRequest>
{
    public ProfileEditValidator()
    {
        RuleFor(x => x.About).MaximumLength(500);
        RuleFor(x => x.CountryCode).MustBeACountryCode();
    }
}
