using Bogus;
using Chess2.Api.Profile.Models;
using Chess2.Api.Tournaments.Entities;
using Chess2.Api.Tournaments.Models;

namespace Chess2.Api.TestInfrastructure.Fakes;

public class TournamentFaker : Faker<Tournament>
{
    public TournamentFaker()
    {
        StrictMode(true);
        RuleFor(x => x.TournamentToken, f => (TournamentToken)f.Random.AlphaNumeric(16));
        RuleFor(x => x.HostedBy, f => (UserId)f.Random.Guid().ToString());
        RuleFor(x => x.BaseSeconds, f => f.Random.Number(60, 600));
        RuleFor(x => x.IncrementSeconds, f => f.Random.Number(0, 10));
        RuleFor(x => x.Format, f => f.PickRandom<TournamentFormat>());
        RuleFor(x => x.HasStarted, false);
    }
}
