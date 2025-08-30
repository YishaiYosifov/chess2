using Chess2.Api.Preferences.Entities;
using Chess2.Api.Preferences.Models;

namespace Chess2.Api.Preferences.DTOs;

public record PreferenceDto(
    bool AllowFriendRequests,
    InteractionLevel ChallengePreference,
    InteractionLevel ChatPreference
)
{
    public PreferenceDto(UserPreferences preferences)
        : this(
            preferences.AllowFriendRequests,
            preferences.ChatPreference,
            preferences.ChatPreference
        ) { }

    public void ApplyTo(UserPreferences preferences)
    {
        preferences.AllowFriendRequests = AllowFriendRequests;
        preferences.ChatPreference = ChatPreference;
        preferences.ChallengePreference = ChallengePreference;
    }
}
