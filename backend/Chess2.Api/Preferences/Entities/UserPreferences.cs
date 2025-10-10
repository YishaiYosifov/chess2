using Chess2.Api.Preferences.Models;
using Chess2.Api.Profile.Models;

namespace Chess2.Api.Preferences.Entities;

public class UserPreferences
{
    public int Id { get; set; }
    public required UserId UserId { get; set; }

    public InteractionLevel ChallengePreference { get; set; } = InteractionLevel.Everyone;
    public bool ShowChat { get; set; } = true;
}
