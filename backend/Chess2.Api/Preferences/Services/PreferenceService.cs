using Chess2.Api.Preferences.DTOs;
using Chess2.Api.Preferences.Repositories;
using Chess2.Api.Profile.Models;
using Chess2.Api.Shared.Services;

namespace Chess2.Api.Preferences.Services;

public interface IPreferenceService
{
    Task<PreferenceDto> GetPreferencesAsync(UserId userId, CancellationToken token = default);
    Task UpdatePreferencesAsync(
        UserId userId,
        PreferenceDto newPreferences,
        CancellationToken token = default
    );
}

public class PreferenceService(IPreferenceRepository preferencesRepository, IUnitOfWork unitOfWork)
    : IPreferenceService
{
    private readonly IPreferenceRepository _preferencesRepository = preferencesRepository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task UpdatePreferencesAsync(
        UserId userId,
        PreferenceDto newPreferences,
        CancellationToken token = default
    )
    {
        var preferences = await _preferencesRepository.GetPreferencesAsync(userId, token);
        if (preferences is null)
        {
            preferences = new() { UserId = userId };
            await _preferencesRepository.AddPreferencesAsync(preferences, token);
        }

        newPreferences.ApplyTo(preferences);
        await _unitOfWork.CompleteAsync(token);
    }

    public async Task<PreferenceDto> GetPreferencesAsync(
        UserId userId,
        CancellationToken token = default
    )
    {
        var preferences =
            await _preferencesRepository.GetPreferencesAsync(userId, token)
            ?? new() { UserId = userId };
        return new(preferences);
    }
}
