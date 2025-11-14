using AnarchyChess.Api.Preferences.DTOs;
using AnarchyChess.Api.Preferences.Models;
using AnarchyChess.Api.Preferences.Services;
using AnarchyChess.Api.Profile.Models;
using AnarchyChess.Api.Social.Services;
using AnarchyChess.Api.TestInfrastructure;
using AnarchyChess.Api.TestInfrastructure.Fakes;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace AnarchyChess.Api.Integration.Tests.Preferences;

public class InteractionLevelGateTests : BaseIntegrationTest
{
    private readonly IInteractionLevelGate _interactionLevelGate;
    private readonly IPreferenceService _preferenceService;
    private readonly IBlockService _blockService;
    private readonly IStarService _starService;

    public InteractionLevelGateTests(AnarchyChessWebApplicationFactory factory)
        : base(factory)
    {
        _interactionLevelGate = Scope.ServiceProvider.GetRequiredService<IInteractionLevelGate>();
        _preferenceService = Scope.ServiceProvider.GetRequiredService<IPreferenceService>();
        _blockService = Scope.ServiceProvider.GetRequiredService<IBlockService>();
        _starService = Scope.ServiceProvider.GetRequiredService<IStarService>();
    }

    [Fact]
    public async Task CanInteractWithAsync_returns_true_if_recipient_accepts_everyone()
    {
        var requester = new AuthedUserFaker().Generate();
        var recipient = new AuthedUserFaker().Generate();
        await DbContext.AddRangeAsync(requester, recipient);
        await DbContext.SaveChangesAsync(CT);

        var prefs = new UserPreferencesFaker(recipient)
            .RuleFor(x => x.ChallengePreference, InteractionLevel.Everyone)
            .Generate();
        await _preferenceService.UpdatePreferencesAsync(recipient.Id, new PreferenceDto(prefs), CT);

        var result = await _interactionLevelGate.CanInteractWithAsync(
            prefs => prefs.ChallengePreference,
            requester.Id,
            recipient.Id
        );
        result.Should().BeTrue();
    }

    [Fact]
    public async Task CanInteractWithAsync_returns_false_if_recipient_is_blocked()
    {
        var requester = new AuthedUserFaker().Generate();
        var recipient = new AuthedUserFaker().Generate();
        await DbContext.AddRangeAsync(requester, recipient);
        await DbContext.SaveChangesAsync(CT);

        var prefs = new UserPreferencesFaker(recipient)
            .RuleFor(x => x.ChallengePreference, InteractionLevel.Everyone)
            .Generate();
        await _preferenceService.UpdatePreferencesAsync(recipient.Id, new PreferenceDto(prefs), CT);
        await _blockService.BlockUserAsync(recipient.Id, requester.Id, CT);

        var result = await _interactionLevelGate.CanInteractWithAsync(
            prefs => prefs.ChallengePreference,
            requester.Id,
            recipient.Id
        );
        result.Should().BeFalse();
    }

    [Fact]
    public async Task CanInteractWithAsync_returns_false_if_recipient_does_not_accept_interactions()
    {
        var requester = new AuthedUserFaker().Generate();
        var recipient = new AuthedUserFaker().Generate();
        await DbContext.AddRangeAsync(requester, recipient);
        await DbContext.SaveChangesAsync(CT);

        var prefs = new UserPreferencesFaker(recipient)
            .RuleFor(x => x.ChallengePreference, InteractionLevel.NoOne)
            .Generate();
        await _preferenceService.UpdatePreferencesAsync(recipient.Id, new PreferenceDto(prefs), CT);

        var result = await _interactionLevelGate.CanInteractWithAsync(
            prefs => prefs.ChallengePreference,
            requester.Id,
            recipient.Id
        );

        result.Should().BeFalse();
    }

    [Fact]
    public async Task CanInteractWithAsync_returns_true_if_recipient_accepts_starred_and_requester_is_starred()
    {
        var requester = new AuthedUserFaker().Generate();
        var recipient = new AuthedUserFaker().Generate();
        await DbContext.AddRangeAsync(requester, recipient);
        await DbContext.SaveChangesAsync(CT);

        var prefs = new UserPreferencesFaker(recipient)
            .RuleFor(x => x.ChallengePreference, InteractionLevel.Starred)
            .Generate();
        await _preferenceService.UpdatePreferencesAsync(recipient.Id, new PreferenceDto(prefs), CT);
        await _starService.AddStarAsync(recipient.Id, requester.Id, CT);

        var result = await _interactionLevelGate.CanInteractWithAsync(
            prefs => prefs.ChallengePreference,
            requester.Id,
            recipient.Id
        );
        result.Should().BeTrue();
    }

    [Fact]
    public async Task CanInteractWithAsync_returns_false_if_recipient_accepts_starred_and_requester_is_not_starred()
    {
        var requester = new AuthedUserFaker().Generate();
        var recipient = new AuthedUserFaker().Generate();
        await DbContext.AddRangeAsync(requester, recipient);
        await DbContext.SaveChangesAsync(CT);

        var prefs = new UserPreferencesFaker(recipient)
            .RuleFor(x => x.ChallengePreference, InteractionLevel.Starred)
            .Generate();
        await _preferenceService.UpdatePreferencesAsync(recipient.Id, new PreferenceDto(prefs), CT);

        var result = await _interactionLevelGate.CanInteractWithAsync(
            prefs => prefs.ChallengePreference,
            requester.Id,
            recipient.Id
        );
        result.Should().BeFalse();
    }

    [Fact]
    public async Task CanInteractWithAsync_returns_false_if_recipient_accepts_starred_but_requester_is_blocked()
    {
        var requester = new AuthedUserFaker().Generate();
        var recipient = new AuthedUserFaker().Generate();
        await DbContext.AddRangeAsync(requester, recipient);
        await DbContext.SaveChangesAsync(CT);

        var prefs = new UserPreferencesFaker(recipient)
            .RuleFor(x => x.ChallengePreference, InteractionLevel.Starred)
            .Generate();
        await _preferenceService.UpdatePreferencesAsync(recipient.Id, new PreferenceDto(prefs), CT);
        await _starService.AddStarAsync(recipient.Id, requester.Id, CT);
        await _blockService.BlockUserAsync(recipient.Id, requester.Id, CT);

        var result = await _interactionLevelGate.CanInteractWithAsync(
            prefs => prefs.ChallengePreference,
            requester.Id,
            recipient.Id
        );
        result.Should().BeFalse();
    }

    [Fact]
    public async Task CanInteractWithAsync_returns_true_if_recipient_accepts_logged_in_and_requester_is_logged_in()
    {
        var requester = new AuthedUserFaker().Generate();
        var recipient = new AuthedUserFaker().Generate();
        await DbContext.AddRangeAsync(requester, recipient);
        await DbContext.SaveChangesAsync(CT);

        var prefs = new UserPreferencesFaker(recipient)
            .RuleFor(x => x.ChallengePreference, InteractionLevel.LoggedIn)
            .Generate();
        await _preferenceService.UpdatePreferencesAsync(recipient.Id, new PreferenceDto(prefs), CT);

        var result = await _interactionLevelGate.CanInteractWithAsync(
            prefs => prefs.ChallengePreference,
            requester.Id,
            recipient.Id
        );

        result.Should().BeTrue();
    }

    [Fact]
    public async Task CanInteractWithAsync_returns_false_if_recipient_accepts_logged_in_and_requester_is_not_logged_in()
    {
        var recipient = new AuthedUserFaker().Generate();
        await DbContext.AddAsync(recipient, CT);
        await DbContext.SaveChangesAsync(CT);

        var prefs = new UserPreferencesFaker(recipient)
            .RuleFor(x => x.ChallengePreference, InteractionLevel.LoggedIn)
            .Generate();
        await _preferenceService.UpdatePreferencesAsync(recipient.Id, new PreferenceDto(prefs), CT);

        var result = await _interactionLevelGate.CanInteractWithAsync(
            prefs => prefs.ChallengePreference,
            UserId.Guest(),
            recipient.Id
        );

        result.Should().BeFalse();
    }
}
