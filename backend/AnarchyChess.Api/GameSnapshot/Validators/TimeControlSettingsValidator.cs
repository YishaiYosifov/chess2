using AnarchyChess.Api.GameSnapshot.Models;
using FluentValidation;

namespace AnarchyChess.Api.GameSnapshot.Validators;

public class TimeControlSettingsValidator : AbstractValidator<TimeControlSettings>
{
    public TimeControlSettingsValidator()
    {
        RuleFor(x => x.BaseSeconds)
            .Must(TimeControlSettings.AllowedBaseSeconds.Contains)
            .WithMessage(
                $"BaseSeconds must be one of {string.Join(", ", TimeControlSettings.AllowedBaseSeconds)}"
            );

        RuleFor(x => x.IncrementSeconds)
            .Must(TimeControlSettings.AllowedIncrementSeconds.Contains)
            .WithMessage(
                $"IncrementSeconds must be one of {string.Join(", ", TimeControlSettings.AllowedIncrementSeconds)}"
            );
    }
}
