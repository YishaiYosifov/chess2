using AnarchyChess.Api.Preferences.DTOs;
using AnarchyChess.Api.Preferences.Models;
using AnarchyChess.Api.TestInfrastructure;
using AnarchyChess.Api.TestInfrastructure.Fakes;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace AnarchyChess.Api.Functional.Tests.PreferenceTests;

public class PreferenceControllerTests(AnarchyChessWebApplicationFactory factory)
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
                    ShowChat: preferences.ShowChat
                )
            );
    }

    [Fact]
    public async Task GetPreferences_with_a_guest_user_returns_default_preferences()
    {
        AuthUtils.AuthenticateGuest(ApiClient);

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
            ShowChat: false
        );

        var response = await ApiClient.Api.SetPreferencesAsync(newPrefs);
        response.IsSuccessful.Should().BeTrue();

        var dbPrefs = await DbContext
            .UserPreferences.AsNoTracking()
            .FirstAsync(p => p.UserId == user.Id, CT);

        dbPrefs.ChallengePreference.Should().Be(newPrefs.ChallengePreference);
        dbPrefs.ShowChat.Should().Be(newPrefs.ShowChat);
    }

    [Fact]
    public async Task SetPreferences_with_a_guest_user_returns_forbidden()
    {
        AuthUtils.AuthenticateGuest(ApiClient);
        PreferenceDto newPrefs = new(
            ChallengePreference: InteractionLevel.Everyone,
            ShowChat: true
        );

        var response = await ApiClient.Api.SetPreferencesAsync(newPrefs);

        response.IsSuccessful.Should().BeFalse();
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
