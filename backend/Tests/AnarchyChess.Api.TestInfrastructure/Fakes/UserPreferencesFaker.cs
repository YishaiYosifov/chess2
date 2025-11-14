using Bogus;
using AnarchyChess.Api.Preferences.Entities;
using AnarchyChess.Api.Preferences.Models;
using AnarchyChess.Api.Profile.Entities;

namespace AnarchyChess.Api.TestInfrastructure.Fakes;

public class UserPreferencesFaker : Faker<UserPreferences>
{
    public UserPreferencesFaker(AuthedUser user)
    {
        StrictMode(true);
        RuleFor(x => x.Id, 0);
        RuleFor(x => x.UserId, user.Id);
        RuleFor(x => x.ChallengePreference, f => f.PickRandom<InteractionLevel>());
        RuleFor(x => x.ShowChat, f => f.Random.Bool());
    }
}
