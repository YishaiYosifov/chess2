using Bogus;
using Chess2.Api.Game.Entities;
using Chess2.Api.GameLogic.Models;

namespace Chess2.Api.TestInfrastructure.Fakes;

public class PlayerArchiveFaker : Faker<PlayerArchive>
{
    public PlayerArchiveFaker(GameColor color)
    {
        StrictMode(true);
        RuleFor(x => x.Id, 0);
        RuleFor(x => x.UserName, f => f.Internet.UserName());
        RuleFor(x => x.UserId, f => f.Random.Guid().ToString());
        RuleFor(x => x.Color, color);
        RuleFor(x => x.FinalTimeRemaining, f => f.Random.Double(0, 100000));
        RuleFor(x => x.InitialRating, f => f.Random.Int(1200, 2500));
        RuleFor(x => x.NewRating, f => f.Random.Int(1200, 2500));
        RuleFor(x => x.CountryCode, f => f.Address.CountryCode());
    }
}
