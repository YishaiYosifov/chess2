using AnarchyChess.Api.GameSnapshot.Models;
using AnarchyChess.Api.GameSnapshot.Validators;
using FluentValidation.TestHelper;

namespace AnarchyChess.Api.Unit.Tests.GameSnapshotTests;

public class TimeControlSettingsRequestValidatorTests
{
    private readonly TimeControlSettingsRequestValidator _validator = new();

    [Theory]
    [InlineData(15)]
    [InlineData(300)]
    [InlineData(5400)]
    public void TimeControlSettingsValidator_accepts_allowed_base_seconds(int baseSeconds)
    {
        TimeControlSettingsRequest model = new(baseSeconds, 0);
        var result = _validator.TestValidate(model);
        result.ShouldNotHaveValidationErrorFor(x => x.BaseSeconds);
    }

    [Theory]
    [InlineData(14)]
    [InlineData(999)]
    [InlineData(6000)]
    public void TimeControlSettingsValidator_rejects_disallowed_base_seconds(int baseSeconds)
    {
        TimeControlSettingsRequest model = new(baseSeconds, 0);
        var result = _validator.TestValidate(model);
        result
            .ShouldHaveValidationErrorFor(x => x.BaseSeconds)
            .WithErrorMessage(
                $"BaseSeconds must be one of {string.Join(", ", TimeControlSettingsRequest.AllowedBaseSeconds)}"
            );
    }

    [Theory]
    [InlineData(0)]
    [InlineData(5)]
    [InlineData(60)]
    public void TimeControlSettingsValidator_accepts_allowed_increment_seconds(int incrementSeconds)
    {
        TimeControlSettingsRequest model = new(60, incrementSeconds);
        var result = _validator.TestValidate(model);
        result.ShouldNotHaveValidationErrorFor(x => x.IncrementSeconds);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(7)]
    [InlineData(100)]
    public void TimeControlSettingsValidator_rejects_disallowed_increment_seconds(
        int incrementSeconds
    )
    {
        TimeControlSettingsRequest model = new(60, incrementSeconds);
        var result = _validator.TestValidate(model);
        result
            .ShouldHaveValidationErrorFor(x => x.IncrementSeconds)
            .WithErrorMessage(
                $"IncrementSeconds must be one of {string.Join(", ", TimeControlSettingsRequest.AllowedIncrementSeconds)}"
            );
    }

    [Fact]
    public void TimeControlSettingsValidator_accepts_valid_settings()
    {
        TimeControlSettingsRequest model = new(300, 10);
        var result = _validator.TestValidate(model);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void TimeControlSettingsValidator_rejects_invalid_settings()
    {
        TimeControlSettingsRequest model = new(999, 7);
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.BaseSeconds);
        result.ShouldHaveValidationErrorFor(x => x.IncrementSeconds);
    }
}
