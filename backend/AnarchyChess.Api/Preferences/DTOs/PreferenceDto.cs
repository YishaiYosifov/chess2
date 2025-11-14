using System.ComponentModel;
using System.Text.Json.Serialization;
using AnarchyChess.Api.Preferences.Entities;
using AnarchyChess.Api.Preferences.Models;

namespace AnarchyChess.Api.Preferences.DTOs;

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
