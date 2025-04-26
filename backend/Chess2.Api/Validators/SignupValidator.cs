using Chess2.Api.Models.DTOs;
using FluentValidation;

namespace Chess2.Api.Validators;

public class SignupValidator : AbstractValidator<SignupRequest>
{
    public SignupValidator()
    {
        RuleFor(x => x.UserName).Length(1, 30).NotEmpty();
        RuleFor(x => x.Email).EmailAddress();
        RuleFor(x => x.Password).MinimumLength(8);
        RuleFor(x => x.CountryCode).MustBeACountryCode().When(x => x.CountryCode is not null);
    }
}
