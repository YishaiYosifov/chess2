using AnarchyChess.Api.Profile.DTOs;
using FluentValidation;

namespace AnarchyChess.Api.Profile.Validators;

public class UsernameEditValidator : AbstractValidator<UsernameEditRequest>
{
    public UsernameEditValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty()
            .Length(3, 30)
            .Matches("^[a-zA-Z0-9-_]+$")
            .WithMessage(
                "Only letters, numbers, hyphens, and underscores are allowed for the username"
            );
    }
}
