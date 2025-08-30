using Chess2.Api.Preferences.Models;

namespace Chess2.Api.Preferences.Entities;

public class UserPreferences
{
    public int Id { get; set; }
    public required string UserId { get; set; }

    public bool AllowFriendRequests { get; set; } = true;
    public InteractionLevel ChallengePreference { get; set; } = InteractionLevel.Everyone;
    public InteractionLevel ChatPreference { get; set; } = InteractionLevel.Everyone;
}
