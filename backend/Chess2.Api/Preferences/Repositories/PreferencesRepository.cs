using Chess2.Api.Infrastructure;
using Chess2.Api.Preferences.Entities;
using Chess2.Api.Users.Models;
using Microsoft.EntityFrameworkCore;

namespace Chess2.Api.Preferences.Repositories;

public interface IPreferencesRepository
{
    Task<UserPreferences?> GetPreferencesAsync(UserId userId, CancellationToken token = default);
    Task UpsertPreferencesAsync(UserPreferences preferences, CancellationToken token = default);
}

public class PreferencesRepository(ApplicationDbContext dbContext) : IPreferencesRepository
{
    private readonly ApplicationDbContext _dbContext = dbContext;

    public async Task<UserPreferences?> GetPreferencesAsync(
        UserId userId,
        CancellationToken token = default
    ) =>
        await _dbContext
            .UserPreferences.Where(preferences => preferences.UserId == userId)
            .FirstOrDefaultAsync(token);

    public async Task UpsertPreferencesAsync(
        UserPreferences preferences,
        CancellationToken token = default
    )
    {
        var existing = await GetPreferencesAsync(preferences.UserId, token);
        if (existing is null)
        {
            await _dbContext.UserPreferences.AddAsync(preferences, token);
        }
        else
        {
            preferences.Id = existing.Id;
            _dbContext.Entry(existing).CurrentValues.SetValues(preferences);
        }
    }
}
