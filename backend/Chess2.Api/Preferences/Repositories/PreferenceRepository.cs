using Chess2.Api.Infrastructure;
using Chess2.Api.Preferences.Entities;
using Microsoft.EntityFrameworkCore;

namespace Chess2.Api.Preferences.Repositories;

public interface IPreferenceRepository
{
    Task<UserPreferences?> GetPreferencesAsync(string userId, CancellationToken token = default);
    Task AddPreferencesAsync(UserPreferences preferences, CancellationToken token = default);
}

public class PreferenceRepository(ApplicationDbContext dbContext) : IPreferenceRepository
{
    private readonly ApplicationDbContext _dbContext = dbContext;

    public async Task<UserPreferences?> GetPreferencesAsync(
        string userId,
        CancellationToken token = default
    ) =>
        await _dbContext
            .UserPreferences.Where(preferences => preferences.UserId == userId)
            .FirstOrDefaultAsync(token);

    public async Task AddPreferencesAsync(
        UserPreferences preferences,
        CancellationToken token = default
    ) => await _dbContext.UserPreferences.AddAsync(preferences, token);
}
