using Chess2.Api.Models.DTOs;
using FluentValidation;

namespace Chess2.Api.Models.Validators;

public class UserValidator : AbstractValidator<UserIn>
{
    public UserValidator()
    {
        RuleFor(x => x.Username).Length(1, 30);
        RuleFor(x => x.Email).EmailAddress();
        RuleFor(x => x.Password).MinimumLength(8);
    }
}
