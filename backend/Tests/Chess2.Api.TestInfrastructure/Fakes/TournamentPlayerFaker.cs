using Bogus;
using Chess2.Api.Profile.Entities;
using Chess2.Api.Profile.Models;
using Chess2.Api.Tournaments.Entities;
using Chess2.Api.Tournaments.Models;

namespace Chess2.Api.TestInfrastructure.Fakes;

public class TournamentPlayerFaker : Faker<TournamentPlayer>
{
    public TournamentPlayerFaker(AuthedUser? user = null)
    {
        StrictMode(true);
        RuleFor(x => x.Id, 0);
        RuleFor(x => x.TournamentToken, f => (TournamentToken)f.Random.AlphaNumeric(16));
        RuleFor(x => x.User, f => user ?? new AuthedUserFaker().Generate());
        RuleFor(x => x.UserId, (f, x) => x.User.Id);
        RuleFor(x => x.LastOpponent, (UserId?)null);
        RuleFor(x => x.Rating, f => f.Random.Number(100, 3000));
        RuleFor(x => x.Score, 0);
    }
}
