using Chess2.Api.Profile.DTOs;
using Chess2.Api.Profile.Entities;
using Chess2.Api.Profile.Errors;
using Chess2.Api.Profile.Services;
using Chess2.Api.Shared.Models;
using Chess2.Api.TestInfrastructure;
using Chess2.Api.TestInfrastructure.Fakes;
using FluentAssertions;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace Chess2.Api.Integration.Tests.ProfileTests;

public class UserSettingsTests : BaseIntegrationTest
{
    private readonly UserSettings _userSettings;
    private readonly AppSettings _settings;
    private readonly TimeProvider _timeProviderMock = Substitute.For<TimeProvider>();

    private readonly DateTime _fakeNow;

    public UserSettingsTests(Chess2WebApplicationFactory factory)
        : base(factory)
    {
        var settings = Scope.ServiceProvider.GetRequiredService<IOptions<AppSettings>>();
        _settings = settings.Value;

        _fakeNow = DateTime.UtcNow;
        _timeProviderMock.GetUtcNow().Returns(new DateTimeOffset(_fakeNow));

        _userSettings = new(
            Scope.ServiceProvider.GetRequiredService<IValidator<ProfileEditRequest>>(),
            Scope.ServiceProvider.GetRequiredService<IValidator<UsernameEditRequest>>(),
            Scope.ServiceProvider.GetRequiredService<UserManager<AuthedUser>>(),
            settings,
            Scope.ServiceProvider.GetRequiredService<ILogger<UserSettings>>(),
            _timeProviderMock
        );
    }

    [Fact]
    public async Task EditProfileAsync_modifies_user_with_valid_data()
    {
        var user = new AuthedUserFaker().Generate();
        await DbContext.AddAsync(user, CT);
        await DbContext.SaveChangesAsync(CT);

        ProfileEditRequest profileEdit = new(About: "New about me", CountryCode: "US");

        var result = await _userSettings.EditProfileAsync(user, profileEdit);

        result.IsError.Should().BeFalse();
        var updatedUser = await DbContext
            .Users.AsNoTracking()
            .SingleAsync(x => x.Id == user.Id, CT);
        updatedUser.About.Should().Be(profileEdit.About);
        updatedUser.CountryCode.Should().Be(profileEdit.CountryCode);
    }

    [Fact]
    public async Task EditProfileAsync_rejects_invalid_data()
    {
        var user = new AuthedUserFaker().Generate();
        await DbContext.AddAsync(user, CT);
        await DbContext.SaveChangesAsync(CT);

        ProfileEditRequest profileEdit = new(About: "", CountryCode: "XZ");

        var result = await _userSettings.EditProfileAsync(user, profileEdit);

        result.IsError.Should().BeTrue();
        var dbUser = await DbContext.Users.AsNoTracking().SingleAsync(x => x.Id == user.Id, CT);
        dbUser.Should().BeEquivalentTo(user);
    }

    [Fact]
    public async Task EditUsernameAsync_allows_change_if_never_changed()
    {
        var user = new AuthedUserFaker()
            .RuleFor(x => x.UsernameLastChanged, (DateTime?)null)
            .Generate();
        await DbContext.AddAsync(user, CT);
        await DbContext.SaveChangesAsync(CT);

        UsernameEditRequest usernameEdit = new("new-username");

        var result = await _userSettings.EditUsernameAsync(user, usernameEdit);

        result.IsError.Should().BeFalse();

        user.UserName = usernameEdit.Username;
        user.UsernameLastChanged = _fakeNow;
        var dbUser = await DbContext.Users.AsNoTracking().SingleAsync(x => x.Id == user.Id, CT);
        dbUser.Should().BeEquivalentTo(user);
    }

    [Fact]
    public async Task EditUsernameAsync_allows_change_if_past_cooldown()
    {
        var user = new AuthedUserFaker()
            .RuleFor(
                x => x.UsernameLastChanged,
                _fakeNow - _settings.UsernameEditCooldown - TimeSpan.FromMinutes(1)
            )
            .Generate();

        await DbContext.AddAsync(user, CT);
        await DbContext.SaveChangesAsync(CT);

        UsernameEditRequest usernameEdit = new("new-username");

        var result = await _userSettings.EditUsernameAsync(user, usernameEdit);

        result.IsError.Should().BeFalse();

        user.UserName = usernameEdit.Username;
        user.UsernameLastChanged = _fakeNow;
        var dbUser = await DbContext.Users.AsNoTracking().SingleAsync(x => x.Id == user.Id, CT);
        dbUser.Should().BeEquivalentTo(user);
    }

    [Fact]
    public async Task EditUsernameAsync_rejects_change_if_within_cooldown()
    {
        var user = new AuthedUserFaker()
            .RuleFor(
                x => x.UsernameLastChanged,
                _fakeNow - TimeSpan.FromTicks(_settings.UsernameEditCooldown.Ticks / 2)
            )
            .Generate();

        await DbContext.AddAsync(user, CT);
        await DbContext.SaveChangesAsync(CT);

        UsernameEditRequest usernameEdit = new("new-username");

        var result = await _userSettings.EditUsernameAsync(user, usernameEdit);

        result.IsError.Should().BeTrue();
        result.FirstError.Should().BeEquivalentTo(ProfileErrors.SettingOnCooldown);
    }

    [Fact]
    public async Task EditUsernameAsync_rejects_invalid_usernames()
    {
        var user = new AuthedUserFaker()
            .RuleFor(x => x.UsernameLastChanged, (DateTime?)null)
            .Generate();

        await DbContext.AddAsync(user, CT);
        await DbContext.SaveChangesAsync(CT);

        UsernameEditRequest usernameEdit = new("inv@l$d username");

        var result = await _userSettings.EditUsernameAsync(user, usernameEdit);

        result.IsError.Should().BeTrue();
    }
}
