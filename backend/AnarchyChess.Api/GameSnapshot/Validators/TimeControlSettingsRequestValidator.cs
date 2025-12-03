using AnarchyChess.Api.GameSnapshot.Models;
using FluentValidation;

namespace AnarchyChess.Api.GameSnapshot.Validators;

public class TimeControlSettingsRequestValidator : AbstractValidator<TimeControlSettingsRequest>
{
    public TimeControlSettingsRequestValidator()
    {
        RuleFor(x => x.BaseSeconds)
            .Must(TimeControlSettingsRequest.AllowedBaseSeconds.Contains)
            .WithMessage(
                $"BaseSeconds must be one of {string.Join(", ", TimeControlSettingsRequest.AllowedBaseSeconds)}"
            );

        RuleFor(x => x.IncrementSeconds)
            .Must(TimeControlSettingsRequest.AllowedIncrementSeconds.Contains)
            .WithMessage(
                $"IncrementSeconds must be one of {string.Join(", ", TimeControlSettingsRequest.AllowedIncrementSeconds)}"
            );
    }
}
