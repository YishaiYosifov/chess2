using Chess2.Api.Preferences.DTOs;
using Chess2.Api.Preferences.Models;
using Chess2.Api.Preferences.Repositories;
using Chess2.Api.Preferences.Services;
using Chess2.Api.Shared.Services;
using Chess2.Api.TestInfrastructure;
using Chess2.Api.TestInfrastructure.Fakes;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Chess2.Api.Integration.Tests.Preferences;

public class PreferenceServiceTests : BaseIntegrationTest
{
    private readonly IPreferenceService _service;
    private readonly IPreferencesRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public PreferenceServiceTests(Chess2WebApplicationFactory factory)
        : base(factory)
    {
        _service = Scope.ServiceProvider.GetRequiredService<IPreferenceService>();
        _repository = Scope.ServiceProvider.GetRequiredService<IPreferencesRepository>();
        _unitOfWork = Scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
    }

    [Fact]
    public async Task GetPreferencesAsync_returns_existing_preferences()
    {
        var user = new AuthedUserFaker().Generate();
        var prefs = new UserPreferencesFaker(user).Generate();

        await DbContext.AddRangeAsync(user, prefs);
        await DbContext.SaveChangesAsync(CT);

        var result = await _service.GetPreferencesAsync(user.Id, CT);

        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(new PreferenceDto(prefs));
    }

    [Fact]
    public async Task GetPreferencesAsync_returns_default_preferences_when_none_exist()
    {
        var user = new AuthedUserFaker().Generate();
        await DbContext.AddAsync(user, CT);
        await DbContext.SaveChangesAsync(CT);

        var result = await _service.GetPreferencesAsync(user.Id, CT);

        result.Should().NotBeNull();
        result
            .Should()
            .BeEquivalentTo(
                new PreferenceDto(
                    AllowFriendRequests: true,
                    ChallengePreference: InteractionLevel.Everyone,
                    ChatPreference: InteractionLevel.Everyone
                )
            );
    }

    [Fact]
    public async Task UpdatePreferencesAsync_updates_existing_preferences()
    {
        var user = new AuthedUserFaker().Generate();
        var prefs = new UserPreferencesFaker(user).Generate();
        await DbContext.AddRangeAsync(user, prefs);
        await DbContext.SaveChangesAsync(CT);

        PreferenceDto newPrefs = new(
            AllowFriendRequests: !prefs.AllowFriendRequests,
            ChallengePreference: InteractionLevel.Friends,
            ChatPreference: InteractionLevel.Everyone
        );

        await _service.UpdatePreferencesAsync(user.Id, newPrefs, CT);

        var updated = await DbContext
            .UserPreferences.AsNoTracking()
            .SingleAsync(p => p.UserId == user.Id, CT);

        updated.Should().BeEquivalentTo(newPrefs, options => options.ExcludingMissingMembers());
    }

    [Fact]
    public async Task UpdatePreferencesAsync_creates_new_preferences_if_none_exist()
    {
        var user = new AuthedUserFaker().Generate();
        await DbContext.AddAsync(user, CT);
        await DbContext.SaveChangesAsync(CT);

        var newDto = new PreferenceDto(
            AllowFriendRequests: true,
            ChallengePreference: InteractionLevel.Everyone,
            ChatPreference: InteractionLevel.Friends
        );

        await _service.UpdatePreferencesAsync(user.Id, newDto, CT);

        var dbPrefs = await DbContext
            .UserPreferences.AsNoTracking()
            .SingleOrDefaultAsync(p => p.UserId == user.Id, CT);

        dbPrefs.Should().NotBeNull();
        dbPrefs.Should().BeEquivalentTo(newDto, options => options.ExcludingMissingMembers());
    }
}
