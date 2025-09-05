using System.Net;
using Chess2.Api.Preferences.DTOs;
using Chess2.Api.Preferences.Models;
using Chess2.Api.TestInfrastructure;
using Chess2.Api.TestInfrastructure.Fakes;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Chess2.Api.Functional.Tests.PreferenceTests;

public class PreferenceControllerTests(Chess2WebApplicationFactory factory)
    : BaseFunctionalTest(factory)
{
    [Fact]
    public async Task GetPreferences_with_an_authed_user()
    {
        var user = (await AuthUtils.AuthenticateAsync(ApiClient)).User;
        var preferences = new UserPreferencesFaker(user).Generate();
        await DbContext.AddAsync(preferences, CT);
        await DbContext.SaveChangesAsync(CT);

        var response = await ApiClient.Api.GetPreferencesAsync();

        response.IsSuccessful.Should().BeTrue();
        response
            .Content.Should()
            .BeEquivalentTo(
                new PreferenceDto(
                    ChallengePreference: preferences.ChallengePreference,
                    ChatPreference: preferences.ChatPreference
                )
            );
    }

    [Fact]
    public async Task GetPreferences_with_a_guest_user_returns_default_preferences()
    {
        AuthUtils.AuthenticateGuest(ApiClient, "guest123");

        var response = await ApiClient.Api.GetPreferencesAsync();

        response.IsSuccessful.Should().BeTrue();
        response.Content.Should().BeEquivalentTo(PreferenceDto.Default);
    }

    [Fact]
    public async Task SetPreferences_updates_preferences()
    {
        var user = (await AuthUtils.AuthenticateAsync(ApiClient)).User;
        PreferenceDto newPrefs = new(
            ChallengePreference: InteractionLevel.Starred,
            ChatPreference: InteractionLevel.NoOne
        );

        var response = await ApiClient.Api.SetPreferencesAsync(newPrefs);

        response.IsSuccessful.Should().BeTrue();

        var dbPrefs = await DbContext
            .UserPreferences.AsNoTracking()
            .FirstAsync(p => p.UserId == user.Id, CT);

        dbPrefs.ChallengePreference.Should().Be(newPrefs.ChallengePreference);
        dbPrefs.ChatPreference.Should().Be(newPrefs.ChatPreference);
    }

    [Fact]
    public async Task SetPreferences_with_a_guest_user_returns_forbidden()
    {
        AuthUtils.AuthenticateGuest(ApiClient, "guest123");
        PreferenceDto newPrefs = new(
            ChallengePreference: InteractionLevel.Everyone,
            ChatPreference: InteractionLevel.Starred
        );

        var response = await ApiClient.Api.SetPreferencesAsync(newPrefs);

        response.IsSuccessful.Should().BeFalse();
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
