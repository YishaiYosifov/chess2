using System.ComponentModel;
using Chess2.Api.Preferences.Entities;
using Chess2.Api.Preferences.Models;
using Newtonsoft.Json;

namespace Chess2.Api.Preferences.DTOs;

[method: JsonConstructor]
[DisplayName("Preferences")]
public record PreferenceDto(InteractionLevel ChallengePreference, InteractionLevel ChatPreference)
{
    public PreferenceDto(UserPreferences preferences)
        : this(
            ChallengePreference: preferences.ChallengePreference,
            ChatPreference: preferences.ChatPreference
        ) { }

    public static PreferenceDto Default => new(new UserPreferences() { UserId = "" });

    public void ApplyTo(UserPreferences preferences)
    {
        preferences.ChatPreference = ChatPreference;
        preferences.ChallengePreference = ChallengePreference;
    }
}
