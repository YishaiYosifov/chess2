using Chess2.Api.Profile.DTOs;
using Chess2.Api.Profile.Entities;
using Chess2.Api.Profile.Errors;
using Chess2.Api.Profile.Services;
using Chess2.Api.Shared.Models;
using Chess2.Api.TestInfrastructure;
using Chess2.Api.TestInfrastructure.Fakes;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace Chess2.Api.Integration.Tests.ProfileTests;

public class ProfileSettingsTests : BaseIntegrationTest
{
    private readonly ProfileSettings _profileSettings;
    private readonly AppSettings _settings;
    private readonly TimeProvider _timeProviderMock = Substitute.For<TimeProvider>();

    private readonly DateTime _fakeNow;

    public ProfileSettingsTests(Chess2WebApplicationFactory factory)
        : base(factory)
    {
        var settings = Scope.ServiceProvider.GetRequiredService<IOptions<AppSettings>>();
        _settings = settings.Value;

        _fakeNow = DateTime.UtcNow;
        _timeProviderMock.GetUtcNow().Returns(new DateTimeOffset(_fakeNow));

        _profileSettings = new(
            Scope.ServiceProvider.GetRequiredService<UserManager<AuthedUser>>(),
            settings,
            Scope.ServiceProvider.GetRequiredService<ILogger<ProfileSettings>>(),
            _timeProviderMock
        );
    }

    [Fact]
    public async Task EditProfileAsync_modifies_user()
    {
        var user = new AuthedUserFaker().Generate();
        await DbContext.AddAsync(user, CT);
        await DbContext.SaveChangesAsync(CT);

        ProfileEditRequest profileEdit = new(About: "New about me", CountryCode: "US");

        var result = await _profileSettings.EditProfileAsync(user, profileEdit);

        result.IsError.Should().BeFalse();
        var updatedUser = await DbContext
            .Users.AsNoTracking()
            .SingleAsync(x => x.Id == user.Id, CT);
        updatedUser.About.Should().Be(profileEdit.About);
        updatedUser.CountryCode.Should().Be(profileEdit.CountryCode);
    }

    [Fact]
    public async Task EditUsernameAsync_allows_change_if_never_changed()
    {
        var user = new AuthedUserFaker()
            .RuleFor(x => x.UsernameLastChanged, (DateTime?)null)
            .Generate();
        await DbContext.AddAsync(user, CT);
        await DbContext.SaveChangesAsync(CT);

        string newUsername = "new-username";

        var result = await _profileSettings.EditUsernameAsync(user, newUsername);

        result.IsError.Should().BeFalse();

        user.UserName = newUsername;
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

        string newUsername = "new-username";

        var result = await _profileSettings.EditUsernameAsync(user, newUsername);

        result.IsError.Should().BeFalse();

        user.UserName = newUsername;
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

        string newUsername = "new-username";

        var result = await _profileSettings.EditUsernameAsync(user, newUsername);

        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(ProfileErrors.SettingOnCooldown);
    }

    [Fact]
    public async Task EditUsernameAsync_rejects_taken_username()
    {
        var userToEdit = new AuthedUserFaker()
            .RuleFor(x => x.UsernameLastChanged, (DateTime?)null)
            .Generate();
        var existingUser = new AuthedUserFaker().Generate();
        await DbContext.AddRangeAsync(userToEdit, existingUser);
        await DbContext.SaveChangesAsync(CT);

        var result = await _profileSettings.EditUsernameAsync(userToEdit, existingUser.UserName!);

        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(ProfileErrors.UserNameTaken);
    }
}
