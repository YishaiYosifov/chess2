using AnarchyChess.Api.Preferences.Models;
using AnarchyChess.Api.Profile.Models;

namespace AnarchyChess.Api.Preferences.Entities;

public class UserPreferences
{
    public int Id { get; set; }
    public required UserId UserId { get; set; }

    public InteractionLevel ChallengePreference { get; set; } = InteractionLevel.Everyone;
    public bool ShowChat { get; set; } = true;
}
