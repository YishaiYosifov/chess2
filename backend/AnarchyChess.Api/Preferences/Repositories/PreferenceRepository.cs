using AnarchyChess.Api.Infrastructure;
using AnarchyChess.Api.Preferences.Entities;
using AnarchyChess.Api.Profile.Models;
using Microsoft.EntityFrameworkCore;

namespace AnarchyChess.Api.Preferences.Repositories;

public interface IPreferenceRepository
{
    Task<UserPreferences?> GetPreferencesAsync(UserId userId, CancellationToken token = default);
    Task AddPreferencesAsync(UserPreferences preferences, CancellationToken token = default);
}

public class PreferenceRepository(ApplicationDbContext dbContext) : IPreferenceRepository
{
    private readonly ApplicationDbContext _dbContext = dbContext;

    public async Task<UserPreferences?> GetPreferencesAsync(
        UserId userId,
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
