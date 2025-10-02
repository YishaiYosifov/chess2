using System.ComponentModel;
using System.Text.Json.Serialization;
using Chess2.Api.Preferences.Entities;
using Chess2.Api.Preferences.Models;

namespace Chess2.Api.Preferences.DTOs;

[method: JsonConstructor]
[DisplayName("Preferences")]
public record PreferenceDto(InteractionLevel ChallengePreference, bool ShowChat)
{
    public PreferenceDto(UserPreferences preferences)
        : this(ChallengePreference: preferences.ChallengePreference, ShowChat: preferences.ShowChat)
    { }

    public static PreferenceDto Default => new(new UserPreferences() { UserId = "" });

    public void ApplyTo(UserPreferences preferences)
    {
        preferences.ShowChat = ShowChat;
        preferences.ChallengePreference = ChallengePreference;
    }
}
