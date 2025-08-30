using Bogus;
using Chess2.Api.Preferences.Entities;
using Chess2.Api.Preferences.Models;
using Chess2.Api.Users.Entities;

namespace Chess2.Api.TestInfrastructure.Fakes;

public class UserPreferencesFaker : Faker<UserPreferences>
{
    public UserPreferencesFaker(AuthedUser user)
    {
        StrictMode(true);
        RuleFor(x => x.Id, 0);
        RuleFor(x => x.UserId, user.Id);
        RuleFor(x => x.AllowFriendRequests, f => f.Random.Bool());
        RuleFor(x => x.ChallengePreference, f => f.PickRandom<InteractionLevel>());
        RuleFor(x => x.ChatPreference, f => f.PickRandom<InteractionLevel>());
    }
}
