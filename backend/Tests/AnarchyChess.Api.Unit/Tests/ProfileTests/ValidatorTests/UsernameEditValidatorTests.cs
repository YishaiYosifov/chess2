using AnarchyChess.Api.Profile.DTOs;
using AnarchyChess.Api.Profile.Validators;
using FluentValidation.TestHelper;

namespace AnarchyChess.Api.Unit.Tests.ProfileTests.ValidatorTests;

public class UsernameEditValidatorTests
{
    private readonly UsernameEditValidator _validator = new();

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void EditUsernameValidator_rejects_empty_usernames(string? username)
    {
        var model = new UsernameEditRequest(Username: username!);
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Username);
    }

    [Theory]
    [InlineData("ab")]
    [InlineData("a")]
    [InlineData("0123456789012345678901234567890")] // 31 chars
    public void EditUsernameValidator_rejects_usernames_with_invalid_length(string username)
    {
        var model = new UsernameEditRequest(Username: username);
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Username);
    }

    [Theory]
    [InlineData("valid_user")]
    [InlineData("Valid-123")]
    [InlineData("user123")]
    public void EditUsernameValidator_accepts_valid_usernames(string username)
    {
        var model = new UsernameEditRequest(Username: username);
        var result = _validator.TestValidate(model);
        result.ShouldNotHaveValidationErrorFor(x => x.Username);
    }

    [Theory]
    [InlineData("invalid user")]
    [InlineData("user!")]
    [InlineData("user@name")]
    [InlineData("user.name")]
    public void EditUsernameValidator_rejects_usernames_with_invalid_characters(string username)
    {
        var model = new UsernameEditRequest(Username: username);
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Username);
    }
}
