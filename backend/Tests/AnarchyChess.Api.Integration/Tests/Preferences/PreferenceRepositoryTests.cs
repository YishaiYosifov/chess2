using AnarchyChess.Api.Preferences.Repositories;
using AnarchyChess.Api.TestInfrastructure;
using AnarchyChess.Api.TestInfrastructure.Fakes;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AnarchyChess.Api.Integration.Tests.Preferences;

public class PreferenceRepositoryTests : BaseIntegrationTest
{
    private readonly IPreferenceRepository _repository;

    public PreferenceRepositoryTests(AnarchyChessWebApplicationFactory factory)
        : base(factory)
    {
        _repository = Scope.ServiceProvider.GetRequiredService<IPreferenceRepository>();
    }

    [Fact]
    public async Task GetPreferencesAsync_finds_the_correct_preferences_for_a_user()
    {
        var userToFind = new AuthedUserFaker().Generate();
        var prefsToFind = new UserPreferencesFaker(userToFind).Generate();

        var otherUser = new AuthedUserFaker().Generate();
        await DbContext.AddRangeAsync(
            userToFind,
            prefsToFind,
            otherUser,
            new UserPreferencesFaker(otherUser).Generate()
        );
        await DbContext.SaveChangesAsync(CT);

        var result = await _repository.GetPreferencesAsync(userToFind.Id, CT);

        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(prefsToFind);
    }

    [Fact]
    public async Task GetPreferencesAsync_returns_null_when_preferences_dont_exist()
    {
        var user = new AuthedUserFaker().Generate();
        await DbContext.AddAsync(user, CT);
        await DbContext.SaveChangesAsync(CT);

        var result = await _repository.GetPreferencesAsync(user.Id, CT);

        result.Should().BeNull();
    }

    [Fact]
    public async Task AddPreferencesAsync_adds_preferences_to_the_user_and_db_context()
    {
        var user = new AuthedUserFaker().Generate();
        await DbContext.AddAsync(user, CT);
        await DbContext.SaveChangesAsync(CT);

        var prefs = new UserPreferencesFaker(user).Generate();

        await _repository.AddPreferencesAsync(prefs, CT);
        await DbContext.SaveChangesAsync(CT);

        var dbPrefs = await DbContext
            .UserPreferences.AsNoTracking()
            .SingleOrDefaultAsync(p => p.UserId == user.Id, CT);

        dbPrefs.Should().NotBeNull();
        dbPrefs.Should().BeEquivalentTo(prefs);
    }
}
