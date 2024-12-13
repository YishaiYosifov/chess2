using Chess2.Api.Models.DTOs;
using FluentValidation;

namespace Chess2.Api.Validators;

public class UserValidator : AbstractValidator<UserIn>
{
    public UserValidator()
    {
        RuleFor(x => x.Username).Length(1, 30).NotEmpty();
        RuleFor(x => x.Email).EmailAddress();
        RuleFor(x => x.Password).MinimumLength(8);
        RuleFor(x => x.CountryCode).MustBeACountryCode().When(x => x.CountryCode is not null);
    }
}
