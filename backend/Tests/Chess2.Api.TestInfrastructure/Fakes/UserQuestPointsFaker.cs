using Bogus;
using Chess2.Api.Profile.Entities;
using Chess2.Api.Quests.Entities;

namespace Chess2.Api.TestInfrastructure.Fakes;

public class UserQuestPointsFaker : Faker<UserQuestPoints>
{
    public UserQuestPointsFaker(AuthedUser? user = null)
    {
        StrictMode(true);
        RuleFor(x => x.Id, 0);
        RuleFor(x => x.User, f => user ?? new AuthedUserFaker().Generate());
        RuleFor(x => x.UserId, (f, x) => x.User.Id);
        RuleFor(x => x.Points, f => f.IndexFaker);
    }
}
